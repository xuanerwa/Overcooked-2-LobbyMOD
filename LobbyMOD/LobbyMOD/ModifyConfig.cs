using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace LobbyMODS
{
    public class ModifyConfig
    {
        public static Harmony HarmonyInstance { get; set; }
        private static ConfigEntry<bool> modify_SingleplayerChopTimeMultiplier;
        public static void Awake()
        {
            modify_SingleplayerChopTimeMultiplier = MODEntry.Instance.Config.Bind<bool>("01-配置修改", "单人切菜倍率(仅街机有效)", false, "单人切菜倍率");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        //mosify SingleplayerChopTimeMultiplier to 1
        [HarmonyPatch(typeof(GameUtils), nameof(GameUtils.GetGameConfig))]
        [HarmonyPostfix]
        private static void GameUtils_GetGameConfig_Postfix(ref GameConfig __result)
        {
            if (__result != null && modify_SingleplayerChopTimeMultiplier.Value && MODEntry.IsInParty)
            {
                __result.SingleplayerChopTimeMultiplier = 1;
            }
        }
    }
}
