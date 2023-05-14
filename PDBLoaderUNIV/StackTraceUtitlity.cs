using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Mono.CompilerServices.SymbolWriter;

public static class StackTracesUtility
{
    internal static Dictionary<Assembly, MonoSymbolFile> CachedMDBFiles = new Dictionary<Assembly, MonoSymbolFile>();
    public static FieldInfo LineNumberInfo = typeof(StackFrame).GetField("lineNumber", AccessTools.all);
    public static FieldInfo FileNameInfo = typeof(StackFrame).GetField("fileName", AccessTools.all);

    internal static bool TryToLoadMonoSymbolFile(Assembly assembly, out MonoSymbolFile symbolFile)
    {
        if (CachedMDBFiles.TryGetValue(assembly, out symbolFile)) return true;
        string text = Path.ChangeExtension(assembly.Location, ".dll.mdb");
        if (!File.Exists(text))
            return false;
        FileStream fileStream = File.OpenRead(text);
        symbolFile = MonoSymbolFile.ReadSymbolFile(fileStream);
        CachedMDBFiles.Add(assembly, symbolFile);
        return true;
			
    }

    public static void ChangeFrame(StackFrame stackFrame)
    {
        MethodBase methodBase = stackFrame?.GetMethod();
        if (methodBase == null)
            return;
        var tryToLoadMonoSymbolFile = TryToLoadMonoSymbolFile(methodBase.Module.Assembly, out var monoSymbolFile);
        if (!tryToLoadMonoSymbolFile) return;
        
        MethodEntry methodByToken = monoSymbolFile.GetMethodByToken(methodBase.MetadataToken);
        int iloffset = stackFrame.GetILOffset();
        foreach (LineNumberEntry lineNumberEntry in methodByToken.GetLineNumberTable().LineNumbers)
        {
            if (lineNumberEntry.Offset == iloffset)
            {
                LineNumberInfo.SetValue(stackFrame, lineNumberEntry.Row);
                FileNameInfo.SetValue(stackFrame, methodByToken.CompileUnit.SourceFile.FileName);
            }
        }
        
    }
}