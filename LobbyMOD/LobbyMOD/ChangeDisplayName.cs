using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Team17.Online;
using static ClientPortalMapNode;

namespace LobbyMODS
{
    public class ChangeDisplayName
    {
        public static ConfigEntry<string> playerName;
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static void Awake()
        {
            playerName = MODEntry.Instance.Config.Bind("01-配置修改", "玩家名字(请去除所有的双引号)", "<size=70><color=#A4A453>惹到我!</color></size><size=45><color=#006F00>你算是</color></size><size=30><color=#F68FED>踢到棉花了</color></size>", "玩家的名字");
            Harmony.CreateAndPatchAll(typeof(ChangeDisplayName));
        }

        [HarmonyPatch(typeof(User), "DisplayName", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool DisplayNameGetterPrefix(ref string __result, User __instance)
        {
            if (__instance.IsLocal)
            {
                __result = "<MOD>" + playerName.Value;
                //log($"读取姓名为: {__result}");
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch(typeof(User), "DisplayName", MethodType.Setter)]
        [HarmonyPrefix]
        public static bool DisplayNameSetterPrefix(ref string value, User __instance)
        {
            if (__instance.IsLocal)
            {
                value = "<MOD>" + playerName.Value;
                //log($"设定姓名为: {value}");
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
