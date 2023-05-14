using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace PDBLoaderUNIV
{
    [HarmonyPatch(typeof(StackTrace), "FrameCount", MethodType.Getter)]
    
    public class StackTraceFrameCountPatch
    {
        public static FieldInfo StackTraceFrames = typeof(StackTrace).GetField("frames", AccessTools.all);
        public static void Prefix(StackTrace __instance, ref int __result)
        {
            var o = (StackFrame[])StackTraceFrames.GetValue(__instance);
            if (__result == 0 && o == null) return;
            foreach (var stackFrame in o) 
                StackTracesUtility.ChangeFrame(stackFrame);
        }
    }
}