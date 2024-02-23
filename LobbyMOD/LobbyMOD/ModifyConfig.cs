using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LobbyMODS
{
    public static class ModifyConfig
    {
        private static ConfigEntry<bool> modify_SingleplayerChopTimeMultiplier;
        public static void Awake()
        {
            modify_SingleplayerChopTimeMultiplier = MODEntry.Instance.Config.Bind<bool>("01-配置修改", "单人切菜倍率(仅街机有效)", false, "单人切菜倍率");
            Harmony.CreateAndPatchAll(typeof(ModifyConfig));
        }

        //mosify SingleplayerChopTimeMultiplier to 1
        [HarmonyPatch(typeof(GameUtils), nameof(GameUtils.GetGameConfig))]
        [HarmonyPostfix]
        private static void GameUtils_GetGameConfig_Postfix(ref GameConfig __result)
        {
            if (__result != null && modify_SingleplayerChopTimeMultiplier.Value && MODEntry.IsHost && MODEntry.playinlobby)
            {
                __result.SingleplayerChopTimeMultiplier = 1;
            }
        }
    }
}
