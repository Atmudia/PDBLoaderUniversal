using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace PDBLoaderUNIV
{
    [HarmonyPatch]
    public static class StackTracePatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(StackTrace).GetConstructors(AccessTools.all);
        }

        [HarmonyPostfix]
        public static void StackTrace_Constructors(StackTrace __instance)
        {
            var frames = __instance.GetFrames();
            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    StackTracesUtility.ChangeFrame(frame);
                }
            }
        }
    }
}
