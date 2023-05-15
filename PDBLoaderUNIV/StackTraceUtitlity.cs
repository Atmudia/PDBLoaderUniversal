using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Mono.CompilerServices.SymbolWriter;
using PDBLoaderUNIV;

public static class StackTracesUtility
{
    private static Dictionary<Assembly, MonoSymbolFile> CachedMdbFiles { get; } = new Dictionary<Assembly, MonoSymbolFile>();
    private static readonly FieldInfo LineNumberInfo = AccessTools.Field(typeof(StackFrame), "lineNumber");
    private static readonly FieldInfo FileNameInfo = AccessTools.Field(typeof(StackFrame), "fileName");

    private static bool TryToLoadMonoSymbolFile(Assembly assembly, out MonoSymbolFile symbolFile)
    {
        symbolFile = null;
    
        if (assembly == null)
        {
            EntryPoint.Log.LogInfo("TryToLoadMonoSymbolFile Failed - Provided assembly is null.");
            return false;
        }
        
        if (assembly.GetName().Name.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase))
        {
            EntryPoint.Log.LogInfo("TryToLoadMonoSymbolFile Skipped - Assembly is Assembly-CSharp.dll.");
            return false;
        }

        if (CachedMdbFiles.TryGetValue(assembly, out symbolFile))
        {
            return symbolFile != null;
        }

        var text = Path.ChangeExtension(assembly.Location, ".dll.mdb");
    
        if (!File.Exists(text))
        {
            EntryPoint.Log.LogInfo($"File does not exist: {text}");
            return false;
        }

        try
        {
            var fileStream = File.OpenRead(text);
            symbolFile = MonoSymbolFile.ReadSymbolFile(fileStream);
            if (symbolFile != null)
            {
                CachedMdbFiles.Add(assembly, symbolFile);
                return true;
            }

            EntryPoint.Log.LogInfo($"Failed to load symbol file: {text}");
            return false;
        }
        catch (Exception ex)
        {
            EntryPoint.Log.LogInfo($"Error occurred during symbol file loading: {ex.Message}");
            return false;
        }
    }

    public static void ChangeFrame(StackFrame stackFrame)
    {
        if (stackFrame == null)
        {
            EntryPoint.Log.LogInfo("ChangeFrame Failed - Provided stackFrame is null.");
            return;
        }
        
        var methodBase = stackFrame.GetMethod();
        if (methodBase == null)
        {
            EntryPoint.Log.LogInfo("ChangeFrame Failed - MethodBase could not be retrieved from the stackFrame.");
            return;
        }

        var assembly = methodBase.Module.Assembly;
        if (assembly.GetName().Name.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase))
        {
            EntryPoint.Log.LogInfo("ChangeFrame Skipped - Assembly is Assembly-CSharp.dll.");
            return;
        }
        
        if (!TryToLoadMonoSymbolFile(methodBase.Module.Assembly, out var monoSymbolFile) || monoSymbolFile == null)
        {
            EntryPoint.Log.LogInfo("Symbol file not found.");
            return;
        }

        var methodByToken = monoSymbolFile.GetMethodByToken(methodBase.MetadataToken);
        if (methodByToken == null)
        {
            EntryPoint.Log.LogInfo("ChangeFrame Failed - Method could not be retrieved by token.");
            return;
        }

        var offset = stackFrame.GetILOffset();
        foreach (var lineNumberEntry in methodByToken.GetLineNumberTable().LineNumbers)
        {
            if (lineNumberEntry.Offset == offset)
            {
                LineNumberInfo.SetValue(stackFrame, lineNumberEntry.Row);
                FileNameInfo.SetValue(stackFrame, methodByToken.CompileUnit.SourceFile.FileName);
                break;
            }
        }
    }
}
