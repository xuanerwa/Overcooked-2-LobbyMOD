using BepInEx.Configuration;
using Steamworks;
using Team17.Online;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using Team17.Online.Multiplayer;
using Team17.Online.Multiplayer.Connection;



namespace HostUtilities
{
    public class KickUser
    {
        public static Harmony HarmonyInstance { get; set; }
        public static List<string> banSteamIdList = new List<string>();
        public static List<string> savedSteamIdList = new List<string>();
        public static string banSteamIdListFilePath = "街机MOD-黑名单.txt";
        public static string savedSteamIdListFilePath = "街机MOD-手动保存的个人信息.txt";
        public static string autoSavedSteamIdListFilePath = "街机MOD-自动保存的个人信息.txt";
        public static ConfigEntry<bool> isAutoKickUser;
        public static ConfigEntry<KeyCode> saveAll;
        public static ConfigEntry<KeyCode> kick2;
        public static ConfigEntry<KeyCode> kick3;
        public static ConfigEntry<KeyCode> kick4;
        public static ConfigEntry<KeyCode> kickAndBan2;
        public static ConfigEntry<KeyCode> kickAndBan3;
        public static ConfigEntry<KeyCode> kickAndBan4;
        public static bool IsInLobby = false;

        public static void Awake()
        {
            isAutoKickUser = _MODEntry.Instance.Config.Bind<bool>("00-功能开关", "自动踢黑名单里的用户", true, "自动踢出在ban列表中的用户");

            kick2 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "04-仅踢出2号位", KeyCode.Alpha2, "按键踢出2号玩家");
            kick3 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "05-仅踢出3号位", KeyCode.Alpha3, "按键踢出3号玩家");
            kick4 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "06-仅踢出4号位", KeyCode.Alpha4, "按键踢出4号玩家");
            kickAndBan2 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "01-拉黑2号位(并踢出)", KeyCode.F2, "拉黑并踢出2号玩家");
            kickAndBan3 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "02-拉黑3号位(并踢出)", KeyCode.F3, "拉黑并踢出3号玩家");
            kickAndBan4 = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "03-拉黑4号位(并踢出)", KeyCode.F4, "拉黑并踢出4号玩家");

            saveAll = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "07-保存当前房间除自己外所有人的主页链接", KeyCode.Alpha5, "保存当前房间除自己外的所有用户主页链接");
            LoadBannedSteamIdList();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(kick2.Value))
            {
                TryKickUser(1, kick2);
            }
            else if (Input.GetKeyDown(kick3.Value))
            {
                TryKickUser(2, kick3);

            }
            else if (Input.GetKeyDown(kick4.Value))
            {
                TryKickUser(3, kick4);

            }
            else if (Input.GetKeyDown(kickAndBan2.Value))
            {
                TryKickUserAndBan(1, kickAndBan2);

            }
            else if (Input.GetKeyDown(kickAndBan3.Value))
            {
                TryKickUserAndBan(2, kickAndBan3);

            }
            else if (Input.GetKeyDown(kickAndBan4.Value))
            {
                TryKickUserAndBan(3, kickAndBan4);
            }
            else if (Input.GetKeyDown(saveAll.Value))
            {
                TrySaveUsersProfileClient();
                _MODEntry.ShowWarningDialog($"主页已保存至 {savedSteamIdListFilePath}");
            }
        }



        static void ShowAllPlayersInfo()
        {
            _MODEntry.LogInfo("--------------------------------------");
            for (int i = 0; i < ServerUserSystem.m_Users.Count; i++)
            {
                User user = ServerUserSystem.m_Users._items[i];
                OnlineUserPlatformId platformID = user.PlatformID;
                bool m_bIsLocal = user.IsLocal;

                _MODEntry.LogInfo($"玩家{i} 昵称:{user.DisplayName} 是否本地:{m_bIsLocal} steamid:{platformID.m_steamId} 主页:https://steamcommunity.com/profiles/{platformID.m_steamId}\n\n\n--------------------------------------");
            }
            _MODEntry.LogInfo("--------------------------------------");
        }

        public static bool KickBanListUser(User user)
        {
            OnlineUserPlatformId platformID = user.PlatformID;
            string steamIdString = platformID.m_steamId.ToString();
            string steamCommunityUrl = $"https://steamcommunity.com/profiles/{steamIdString}";
            string steamCommunityUrlWithSplash = $"https://steamcommunity.com/profiles/{steamIdString}/";
            var processedList = banSteamIdList.Select(id => id.Split(',')[0]).ToList();
            if (processedList.Contains(steamIdString) ||
                processedList.Contains(steamCommunityUrl) ||
                processedList.Contains(steamCommunityUrlWithSplash
                ))
            {
                _MODEntry.LogInfo($"自动移除  主页: {steamCommunityUrl}  昵称: {user.DisplayName}");
                UI_DisplayKickedUser.add_m_Text($"自动移除  {user.DisplayName}");
                //SteamNetworking.CloseP2PSessionWithUser(platformID.m_steamId);
                ServerUserSystem.RemoveUser(user, true);
                _MODEntry.ShowWarningDialog($"自动移除黑名单  {user.DisplayName}");
                return true;

            }

            return false;
        }

        public static void TryKickUser(int index, ConfigEntry<KeyCode> kickKey)
        {
            if (ServerUserSystem.m_Users.Count > index)
            {
                User user = ServerUserSystem.m_Users._items[index];
                bool m_bIsLocal = user.IsLocal;
                _MODEntry.LogInfo($"尝试移除{index + 1}号:{user.DisplayName}");
                if (!m_bIsLocal)
                {
                    OnlineUserPlatformId platformID = user.PlatformID;
                    //SteamNetworking.CloseP2PSessionWithUser(platformID.m_steamId);
                    ServerUserSystem.RemoveUser(user, true);
                    _MODEntry.LogInfo($"{index + 1}号移除成功:{user.DisplayName}, Steamid:{platformID.m_steamId}");
                }
                else
                {
                    _MODEntry.LogInfo($"{index + 1}号移除失败:{user.DisplayName}, 本地玩家");
                }
            }
        }
        public static void TryKickUserAndBan(int index, ConfigEntry<KeyCode> kickKey)
        {
            if (ServerUserSystem.m_Users.Count > index)
            {
                User user = ServerUserSystem.m_Users._items[index];
                bool m_bIsLocal = user.IsLocal;
                _MODEntry.LogInfo($"尝试移除{index + 1}号:{user.DisplayName}");
                if (!m_bIsLocal)
                {
                    _MODEntry.LogInfo($"{index + 1} 号移除成功: {user.DisplayName} 并拉黑");

                    OnlineUserPlatformId platformID = user.PlatformID;
                    //SteamNetworking.CloseP2PSessionWithUser(platformID.m_steamId);
                    ServerUserSystem.RemoveUser(user, true);

                    string steamIdString = platformID.m_steamId.ToString();
                    string steamCommunityUrl = $"https://steamcommunity.com/profiles/{steamIdString},{user.DisplayName}";
                    banSteamIdList.Add(steamCommunityUrl);
                    SaveBannedSteamIdList();
                    LoadBannedSteamIdList();
                }
                else
                {
                    _MODEntry.LogInfo($"{index + 1}号移除失败:{user.DisplayName}, 本地玩家");
                }
            }
        }

        public static void TrySaveUsersProfile()
        {
            if (ServerUserSystem.m_Users.Count > 1)
            {
                DateTime currentTime = DateTime.Now;
                string formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                savedSteamIdList.Add($"-----------{formattedTime}----------");
                for (var index = 1; index < ServerUserSystem.m_Users.Count; index++)
                {
                    User user = ServerUserSystem.m_Users._items[index];
                    _MODEntry.LogInfo($"保存:{user.DisplayName}");
                    OnlineUserPlatformId platformID = user.PlatformID;
                    CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                    string steamIdString = csteamID.ToString();
                    string steamCommunityUrl = $"steam主页链接: https://steamcommunity.com/profiles/{steamIdString} ,昵称: {user.DisplayName}  ";
                    savedSteamIdList.Add(steamCommunityUrl);
                }
                savedSteamIdList.Add("---------------------------------------------");
                SaveSavedSteamIdList(savedSteamIdList.ToArray());
                savedSteamIdList.Clear();
            }

        }
        public static void TrySaveUsersProfileClient()
        {
            if (ClientUserSystem.m_Users.Count > 1)
            {
                DateTime currentTime = DateTime.Now;
                string formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                savedSteamIdList.Add($"------------{formattedTime}-----------");
                for (var index = 0; index < ClientUserSystem.m_Users.Count; index++)
                {
                    User user = ClientUserSystem.m_Users._items[index];
                    if (user.IsLocal != true)
                    {

                        _MODEntry.LogInfo($"保存:{user.DisplayName}");
                        OnlineUserPlatformId platformID = user.PlatformID;
                        CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                        string steamIdString = csteamID.ToString();
                        string steamCommunityUrl = $"steam主页链接: https://steamcommunity.com/profiles/{steamIdString} ,昵称: {user.DisplayName}  ";
                        savedSteamIdList.Add(steamCommunityUrl);
                    }
                }
                savedSteamIdList.Add("---------------------------------------------");
                SaveSavedSteamIdList(savedSteamIdList.ToArray());
                savedSteamIdList.Clear();
            }

        }

        public static void LoadBannedSteamIdList()
        {
            if (File.Exists(banSteamIdListFilePath))
            {
                string[] lines = File.ReadAllLines(banSteamIdListFilePath);
                banSteamIdList = new List<string>(lines);
            }
        }


        public static void SaveBannedSteamIdList()
        {
            File.WriteAllLines(banSteamIdListFilePath, banSteamIdList.ToArray());
        }


        public static void SaveSavedSteamIdList(string[] manualSavedSteamIdList)
        {
            string contentToAppend = string.Join(Environment.NewLine, manualSavedSteamIdList);
            File.AppendAllText(savedSteamIdListFilePath, contentToAppend + Environment.NewLine);
        }


        public static void SaveAutoSavedSteamIdList(string[] autoSavedSteamIdList)
        {
            string contentToAppend = string.Join(Environment.NewLine, autoSavedSteamIdList);
            File.AppendAllText(autoSavedSteamIdListFilePath, contentToAppend + Environment.NewLine);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ServerUserSystem), "AddUser")]
        public static void ServerUserSystem_AddUser_Patch(User.MachineID machine, EngagementSlot engagement)
        {
            if (_MODEntry.IsHost)
            {
                FastList<User> users = ServerUserSystem.m_Users;
                User user = UserSystemUtils.FindUser(users, null, machine, engagement, TeamID.Count, User.SplitStatus.Count);
                if (isAutoKickUser.Value)
                {
                    if (_MODEntry.IsInLobby)
                    {
                        bool isKicked = KickBanListUser(user);
                        if (isKicked)
                        {
                            return;
                        }
                    }
                }
                if (!user.IsLocal)
                {
                    _MODEntry.LogInfo($"保存:{user.DisplayName}");
                    OnlineUserPlatformId platformID = user.PlatformID;
                    CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                    string steamIdString = csteamID.ToString();
                    string steamCommunityUrl = $"steam主页链接: https://steamcommunity.com/profiles/{steamIdString}";

                    if (steamIdString == "76561198415369188")
                    {
                        if (_MODEntry.IsInParty)
                        {
                            ServerUserSystem.RemoveUser(user, true);
                        }
                    }

                    DateTime currentTime = DateTime.Now;
                    string formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string[] autoSavedSteamIdList = new string[]
                    {
                    $"------------{formattedTime}-----------",
                    $"steam显示昵称: {user.DisplayName}",steamCommunityUrl,
                    "---------------------------------------------"
                    };
                    SaveAutoSavedSteamIdList(autoSavedSteamIdList);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPlayerMenuBehaviour), "UpdateMenuStructure")]
        public static bool UIPlayerMenuBehaviour_UpdateMenuStructure_Prefix(UIPlayerMenuBehaviour __instance)
        {

            if (!_MODEntry.IsHost)
            {
                return true;
            }

            List<UIPlayerMenuBehaviour.MenuOption> menuOptions = __instance.m_menuOptions;

            User m_User = __instance.m_User;
            for (int i = 0; i < menuOptions.Count; i++)
            {
                bool flag = true;
                if (m_User == null)
                {
                    flag = false;
                }
                UIPlayerMenuBehaviour.UIPlayerMenuOptions type = menuOptions[i].m_type;
                if (type == UIPlayerMenuBehaviour.UIPlayerMenuOptions.Mute || type == UIPlayerMenuBehaviour.UIPlayerMenuOptions.Unmute)
                {
                    flag = false;
                }
                if (type == UIPlayerMenuBehaviour.UIPlayerMenuOptions.Kick && m_User.IsLocal)
                {
                    flag = false;
                }
                menuOptions[i].m_button.gameObject.SetActive(flag);
            }
            _MODEntry.LogWarning("已patch  UpdateMenuStructure");
            __instance.UpdateNavigation();
            return false;
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPlayerMenuBehaviour), "KickUser")]
        public static bool UIPlayerMenuBehaviour_KickUser_Prefix(UIPlayerMenuBehaviour __instance)
        {
            HandleKickUserAsync(__instance);
            return false;
        }


        public static void HandleKickUserAsync(UIPlayerMenuBehaviour __instance)
        {
            MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
            if (multiplayerController == null)
            {
                _MODEntry.LogInfo("实例不存在");
                return;
            }
            // 寻找user
            User m_User = __instance.m_User;
            int num = ClientUserSystem.m_Users.FindIndex((User x) => x == m_User);
            User user = ServerUserSystem.m_Users._items[num];
            //OnlineUserPlatformId platformID = user.PlatformID;

            //Server LocalServer = m_LocalServer.GetValue(multiplayerController) as Server;
            //Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection> RemoteClientConnectionsDict = m_RemoteClientConnections.GetValue(LocalServer) as Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection>;
            //SteamNetworking.CloseP2PSessionWithUser(platformID.m_steamId);

            //{
            //    IOnlineMultiplayerSessionUserId sessionId = user.SessionId;

            //    if (sessionId != null && RemoteClientConnectionsDict.ContainsKey(sessionId))
            //    {
            //        NetworkConnection networkConnection = RemoteClientConnectionsDict[sessionId];
            //        LocalServer.HandleDisconnectMessage(networkConnection);
            //        //        object[] parameters = new object[] { sessionId, networkConnection };
            //        //        RemoveConnection.Invoke(LocalServer, parameters);
            //    }
            //}

            ServerUserSystem.RemoveUser(user, true);
            //OnUserRemoved.Invoke(LocalServer, new object[] { user });
        }
       
    }
}
