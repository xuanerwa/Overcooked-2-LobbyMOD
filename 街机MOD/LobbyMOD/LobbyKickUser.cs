using BepInEx;
using BepInEx.Configuration;
using Steamworks;
using Team17.Online;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace LobbyMODS
{
    public class LobbyKickUser
    {

        public static List<string> banSteamIdList = new List<string>();
        public static List<string> savedSteamIdList = new List<string>();
        public static string banSteamIdListFilePath = "0-黑名单-最后一行空行要删掉.txt";
        public static string savedSteamIdListFilePath = "0-保存的steam主页链接.txt";
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
            isAutoKickUser = MODEntry.Instance.Config.Bind<bool>("00-功能开关", "自动踢黑名单里的用户", true, "自动踢出在ban列表中的用户");

            kick2 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "04-踢2号位", KeyCode.Alpha2, "按键踢出2号玩家");
            kick3 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "05-踢3号位", KeyCode.Alpha3, "按键踢出3号玩家");
            kick4 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "06-踢4号位", KeyCode.Alpha4, "按键踢出4号玩家");
            kickAndBan2 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "01-踢2号位并拉黑", KeyCode.F2, "按键踢出2号玩家");
            kickAndBan3 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "02-踢3号位并拉黑", KeyCode.F3, "按键踢出3号玩家");
            kickAndBan4 = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "03-踢4号位并拉黑", KeyCode.F4, "按键踢出4号玩家");

            saveAll = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "07-保存当前房间所有用户的主页链接", KeyCode.Alpha5, "保存当前房间用户主页链接");
            LoadSteamIdList();
            Harmony.CreateAndPatchAll(typeof(LobbyKickUser));
        }

        public static void Update()
        {
            //踢人
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

        }



        static void ShowAllPlayersInfo()
        {
            MODEntry.LogInfo("--------------------------------------");
            for (int i = 0; i < ServerUserSystem.m_Users.Count; i++)
            {
                User user = ServerUserSystem.m_Users._items[i];
                OnlineUserPlatformId platformID = user.PlatformID;
                bool m_bIsLocal = user.IsLocal;
                CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                MODEntry.LogInfo($"玩家{i} 昵称:{user.DisplayName} 是否本地:{m_bIsLocal} steamid:{csteamID} 主页:https://steamcommunity.com/profiles/{csteamID}\n\n\n--------------------------------------");
            }
            MODEntry.LogInfo("--------------------------------------");
        }

        public static void KickBanListUser()
        {
            FastList<User> m_users = ServerUserSystem.m_Users;
            for (int i = 0; i < m_users.Count; i++)
            {
                User user = m_users._items[i];
                OnlineUserPlatformId platformID = user.PlatformID;
                CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                String steamIdString = csteamID.ToString();
                string steamCommunityUrl = $"https://steamcommunity.com/profiles/{steamIdString}";
                string steamCommunityUrlWithSplash = $"https://steamcommunity.com/profiles/{steamIdString}/";
                var processedList = banSteamIdList.Select(id => id.Split(',')[0]).ToList();
                if (processedList.Contains(steamIdString) ||
                    processedList.Contains(steamCommunityUrl) ||
                    processedList.Contains(steamCommunityUrlWithSplash
                    ))
                {
                    MODEntry.LogInfo($"自动移除  主页: {steamCommunityUrl}  昵称: {user.DisplayName}");
                    DisplayKickedUser.add_m_Text($"自动移除  {user.DisplayName}");
                    ServerUserSystem.RemoveUser(user, true);
                    break;  // 如果找到一个，移除后退出循环 
                }
            }
        }

        public static void TryKickUser(int index, ConfigEntry<KeyCode> kickKey)
        {
            if (ServerUserSystem.m_Users.Count > index)
            {
                User user = ServerUserSystem.m_Users._items[index];
                bool m_bIsLocal = user.IsLocal;
                MODEntry.LogInfo($"尝试移除{index + 1}号:{user.DisplayName}");
                if (!m_bIsLocal)
                {
                    ServerUserSystem.RemoveUser(user, true);
                    MODEntry.LogInfo($"{index + 1}号移除成功:{user.DisplayName}");
                }
                else
                {
                    MODEntry.LogInfo($"{index + 1}号移除失败:{user.DisplayName}, 本地玩家");
                }
            }
        }
        public static void TryKickUserAndBan(int index, ConfigEntry<KeyCode> kickKey)
        {
            if (ServerUserSystem.m_Users.Count > index)
            {
                User user = ServerUserSystem.m_Users._items[index];
                bool m_bIsLocal = user.IsLocal;
                MODEntry.LogInfo($"尝试移除{index + 1}号:{user.DisplayName}");
                if (!m_bIsLocal)
                {
                    ServerUserSystem.RemoveUser(user, true);
                    MODEntry.LogInfo($"{index + 1} 号移除成功: {user.DisplayName} 并拉黑");

                    OnlineUserPlatformId platformID = user.PlatformID;
                    CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                    String steamIdString = csteamID.ToString();
                    string steamCommunityUrl = $"https://steamcommunity.com/profiles/{steamIdString},{user.DisplayName}";
                    banSteamIdList.Add(steamCommunityUrl);
                    SaveSteamIdList();
                    LoadSteamIdList();
                }
                else
                {
                    MODEntry.LogInfo($"{index + 1}号移除失败:{user.DisplayName}, 本地玩家");
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
                    MODEntry.LogInfo($"保存:{user.DisplayName}");
                    OnlineUserPlatformId platformID = user.PlatformID;
                    CSteamID? csteamID = (platformID != null) ? new CSteamID?(platformID.m_steamId) : null;
                    String steamIdString = csteamID.ToString();
                    string steamCommunityUrl = $"steam主页链接: https://steamcommunity.com/profiles/{steamIdString} ,昵称: {user.DisplayName}  ";
                    savedSteamIdList.Add(steamCommunityUrl);
                }
                savedSteamIdList.Add("---------------------------------------------");
                SaveSavedSteamIdList();
                LoadSavedSteamIdList();
            }

        }

        public static void LoadSteamIdList()
        {
            if (File.Exists(banSteamIdListFilePath))
            {
                string[] lines = File.ReadAllLines(banSteamIdListFilePath);
                banSteamIdList = new List<string>(lines);
            }
        }

        public static void LoadSavedSteamIdList()
        {
            if (File.Exists(savedSteamIdListFilePath))
            {
                string[] lines = File.ReadAllLines(savedSteamIdListFilePath);
                savedSteamIdList = new List<string>(lines);
            }
        }

        public static void SaveSteamIdList()
        {
            File.WriteAllLines(banSteamIdListFilePath, banSteamIdList.ToArray());
        }

        public static void SaveSavedSteamIdList()
        {
            File.WriteAllLines(savedSteamIdListFilePath, savedSteamIdList.ToArray());
        }

        //添加客机时自动踢人
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ServerUserSystem), "AddUser")]
        public static void ServerUserSystem_AddUser_Patch()
        {
            if (isAutoKickUser.Value)
            {
                if (MODEntry.IsInLobby)
                {
                    MODEntry.LogInfo("踢黑名单");
                    KickBanListUser();
                }
            }
        }

    }
}
