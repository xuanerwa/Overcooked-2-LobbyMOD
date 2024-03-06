using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
using static ClientPortalMapNode;
namespace LobbyMODS
{
    public class ChangeDisplayName
    {
        public static Harmony HarmonyInstance { get; set; }
        public static ConfigEntry<string> playerName;
        public static string lastPlayerName;
        public static ConfigEntry<bool> isReplaceName;
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static void Awake()
        {
            playerName = MODEntry.Instance.Config.Bind("00-UI字体", "替换名字(说明请鼠标悬浮查看)", "<size=45><color=#00FFFF>惹到我!</color></size><size=30><color=#00CF00>你算是</color></size><size=15><color=#F68FED>踢到棉花了</color></size>", "主机修改延迟1秒立刻生效,客机也能看到生效的新名字,客机修改名字需要重新加入战局,或者重新进入街机.");
            isReplaceName = MODEntry.Instance.Config.Bind<bool>("00-功能开关", "UI-替换名字(关闭须重启游戏)", false);
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            if (isReplaceName.Value)
            {
                if (lastPlayerName != playerName.Value)
                {
                    FastList<User> serveruser = ServerUserSystem.m_Users;
                    for (int i = 0; i < serveruser.Count; i++)
                    {
                        if (serveruser._items[i].IsLocal)
                            serveruser._items[i].DisplayName =
                                "<MOD>" +
                                playerName.Value;
                    }
                    FastList<User> clientuser = ClientUserSystem.m_Users;
                    for (int i = 0; i < clientuser.Count; i++)
                    {
                        if (clientuser._items[i].IsLocal)
                            clientuser._items[i].DisplayName =
                                "<MOD>" +
                                playerName.Value;
                    }
                }
            }
        }


        [HarmonyPatch(typeof(User), "DisplayName", MethodType.Getter)]
        [HarmonyPostfix]
        public static void DisplayName_Getter_Prefix(ref string __result, User __instance)
        {
            if (isReplaceName.Value)
            {
                if (__instance.IsLocal)
                {
                    __result =
                        "<MOD>" +
                        playerName.Value;
                }
            }
        }

        [HarmonyPatch(typeof(User), "DisplayName", MethodType.Setter)]
        [HarmonyPrefix]
        public static void DisplayNameSetterPrefix(ref string value, User __instance)
        {
            if (isReplaceName.Value)
            {
                if (__instance.IsLocal)
                {
                    value =
                        "<MOD>" +
                        playerName.Value;

                }
            }
        }

        [HarmonyPatch(typeof(JoinDataProvider), "BuildJoinRequestData")]
        [HarmonyPrefix]
        public static void JoinDataProvider_BuildJoinRequestData_Prefix(ref OnlineMultiplayerLocalUserId requestingLocalUser)
        {
            if (isReplaceName.Value)
            {
                requestingLocalUser.m_userName =
                "<MOD>" +
                playerName.Value;
            }
        }

    }
}
