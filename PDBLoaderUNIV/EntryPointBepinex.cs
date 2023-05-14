using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PDBLoaderUNIV
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class EntryPoint : BaseUnityPlugin
    {
        private const string pluginGuid = "pdbloaderuniv.universal.komikspl";
        private const string pluginName = "PDBLoaderUniversal";
        private const string pluginVersion = "1.0.0";
        public static bool isIntialized;

        public static string pdb2mdbPath = Path.Combine(Application.dataPath, "pdb2mdb.exe");

        public void Awake()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer)
                throw new Exception("The PDBLoader works only for Windows");
            Harmony harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "PDBLoader");
            Logger.LogInfo("Checking if pdb2mdb is installed...");
            Logger.LogInfo($"Path: {pdb2mdbPath}");
            if (!isIntialized)
            {
                var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("0PDBLoaderUNIV.pdb2mdb.exe");
                using (manifestResourceStream)
                {
                    if (manifestResourceStream == null) return;
                    byte[] ba = new byte[manifestResourceStream.Length];
                    var read = manifestResourceStream.Read(ba, 0, ba.Length);
                    File.WriteAllBytes(pdb2mdbPath, ba);
                }
            }
            Logger.LogInfo($"Installed");
            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.dll"))
            {
                var changeExtension = Path.ChangeExtension(file, ".pdb");
                if (File.Exists(changeExtension))
                {
                    Logger.LogInfo(file);
                    var process = Process.Start(pdb2mdbPath, "\"" + file + "\"");
                }
            }
            

        }
    }
}