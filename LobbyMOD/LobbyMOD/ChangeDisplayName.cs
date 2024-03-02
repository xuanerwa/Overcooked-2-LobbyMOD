using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Team17.Online;
namespace LobbyMODS
{
    public class ChangeDisplayName
    {
        public static ConfigEntry<string> playerName;
        public static ConfigEntry<bool> isReplaceName;
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static void Awake()
        {
            playerName = MODEntry.Instance.Config.Bind("00-UI字体", "替换名字(仅主机才生效,删除引号)", "<size=45><color=#00FFFF>惹到我!</color></size><size=30><color=#00CF00>你算是</color></size><size=15><color=#F68FED>踢到棉花了</color></size>", "玩家的名字");
            isReplaceName = MODEntry.Instance.Config.Bind<bool>("00-功能开关", "UI-替换名字(关闭须重启游戏)", false);
            Harmony.CreateAndPatchAll(typeof(ChangeDisplayName));
        }
        public static void Update()
        {
            if (isReplaceName.Value)
            {
                FastList<User> serveruser = ServerUserSystem.m_Users;
                for (int i = 0; i < serveruser.Count; i++)
                {
                    if (serveruser._items[i].IsLocal)
                        serveruser._items[i].DisplayName = "<MOD>" + playerName.Value;
                }
                FastList<User> clientuser = ClientUserSystem.m_Users;
                for (int i = 0; i < clientuser.Count; i++)
                {
                    if (clientuser._items[i].IsLocal)
                        clientuser._items[i].DisplayName = "<MOD>" + playerName.Value;
                }
            }
        }
        //[HarmonyPatch(typeof(User), "DisplayName", MethodType.Getter)]
        //[HarmonyPrefix]
        //public static bool DisplayNameGetterPrefix(ref string __result, User __instance)
        //{
        //    if (__instance.IsLocal)
        //    {
        //        playerName.Value = playerName.Value.Replace("\"", "");
        //        __result = "<MOD>" + playerName.Value;
        //        //log($"读取姓名为: {__result}");
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(User), "DisplayName", MethodType.Setter)]
        //[HarmonyPrefix]
        //public static bool DisplayNameSetterPrefix(ref string value, User __instance)
        //{
        //    if (__instance.IsLocal)
        //    {
        //        playerName.Value = playerName.Value.Replace("\"", "");
        //        value = "<MOD>" + playerName.Value;
        //        //log($"设定姓名为: {value}");
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

    }
}
