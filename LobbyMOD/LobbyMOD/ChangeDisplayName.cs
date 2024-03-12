using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
namespace HostPartyMODs
{
    public class ChangeDisplayName
    {
        public static Harmony HarmonyInstance { get; set; }
        public static ConfigEntry<string> playerName;
        public static string lastPlayerName;
        public static ConfigEntry<bool> isReplaceName;
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        private static string PromptWords = "";
        public static void Awake()
        {
            playerName = _MODEntry.Instance.Config.Bind("00-UI", "01-替换名字(第一次使用请鼠标悬浮查看说明)", "<size=45><color=#00FFFF>MOD群 164509805</color></size>", "请提前删除所有的双引号. 主机修改立刻生效,客机也能看到生效的新名字,客机修改名字需要重新加入战局,或者重新进入街机. 请注意!!!客机名字长度有限制,主机没有长度限制,如果客机进入战局名字空白,请尝试改短一点设置的名字!");
            isReplaceName = _MODEntry.Instance.Config.Bind<bool>("00-UI", "00-替换名字开关(关闭须重启游戏)", false);
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
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
                                PromptWords +
                                playerName.Value;
                    }
                    FastList<User> clientuser = ClientUserSystem.m_Users;
                    for (int i = 0; i < clientuser.Count; i++)
                    {
                        if (clientuser._items[i].IsLocal)
                            clientuser._items[i].DisplayName =
                                PromptWords +
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
                        PromptWords +
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
                        PromptWords +
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
                PromptWords +
                playerName.Value;
            }
        }
    }
}
