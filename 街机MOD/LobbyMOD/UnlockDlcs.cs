using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyMODS
{
    public class UnlockDlcs
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(UnlockDlcs));
        }

        [HarmonyPatch(typeof(DLCManagerBase), "IsDLCAvailable")]
        [HarmonyPostfix]
        public static void IsDLCAvailable(ref bool __result)
        {
            __result = true;
        }
    }
}
