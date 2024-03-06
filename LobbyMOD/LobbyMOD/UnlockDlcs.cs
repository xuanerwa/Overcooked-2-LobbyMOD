using HarmonyLib;
using System;
using System.Reflection;

namespace LobbyMODS
{
    public class UnlockDlcs
    {
        public static Harmony HarmonyInstance { get; set; }
        public static void Awake()
        {
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPatch(typeof(DLCManagerBase), "IsDLCAvailable")]
        [HarmonyPostfix]
        public static void IsDLCAvailable(ref bool __result)
        {
            __result = true;
        }
    }
}
