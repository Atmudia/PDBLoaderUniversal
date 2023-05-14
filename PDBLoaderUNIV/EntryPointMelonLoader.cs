using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Mono.CompilerServices.SymbolWriter;
using UnityEngine;




[assembly: MelonInfo(typeof(PDBLoaderUNIV.EntryPoint), "PDBLoaderUniversal", "1.0.0", "KomiksPL")]
namespace PDBLoaderUNIV
{
    public class EntryPoint : MelonMod
    {
        public static bool isIntialized;

        public static string pdb2mdbPath = Path.Combine(Application.dataPath, "pdb2mdb.exe");

        public override void OnInitializeMelon()
        {
            
            if (Application.platform != RuntimePlatform.WindowsPlayer)
                throw new Exception("The PDBLoader works only for Windows");
                


            
            MelonLogger.Msg("Checking if pdb2mdb is installed...");
            MelonLogger.Msg($"Path: {pdb2mdbPath}");
            
            
            
            if (!isIntialized)
            {
                
                var manifestResourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("0PDBLoaderUNIV.pdb2mdb.exe");
                using (manifestResourceStream)
                {
                    if (manifestResourceStream == null) return;
                    byte[] ba = new byte[manifestResourceStream.Length];
                    var read = manifestResourceStream.Read(ba, 0, ba.Length);
                    File.WriteAllBytes(pdb2mdbPath, ba);
                }
            }
            
            MelonLogger.Msg($"Installed");
            foreach (var file in Directory.GetFiles(MelonEnvironment.ModsDirectory, "*.dll"))
            {
                var changeExtension = Path.ChangeExtension(file, ".pdb");
                if (File.Exists(changeExtension))
                {
                    MelonLogger.Msg(file);
                    var process = Process.Start(pdb2mdbPath, "\"" + file + "\"");
                }
            }
            


        }
        
    }
}