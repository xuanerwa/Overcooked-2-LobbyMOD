using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Version = System.Version;
using UnityEngine.UI;

namespace HostUtilities
{
    [BepInPlugin("com.ch3ngyz.plugin.HostUtilities", "[HostUtilities] By.yc阿哲 Q群860480677 点击下方“‧‧‧”展开", "1.0.74")]
    [BepInProcess("Overcooked2.exe")]
    public class _MODEntry : BaseUnityPlugin
    {
        public static string Version = "1.0.74";
        public static Harmony HarmonyInstance { get; set; }
        public static List<string> AllHarmonyName = new List<string>();
        public static List<Harmony> AllHarmony = new List<Harmony>();
        public static string modName;
        public static _MODEntry Instance;
        public static bool IsInLobby = false;
        public static bool IsHost = false;
        public static bool IsInParty = false;
        public static float dpiScaleFactor = 1f;
        private float baseScreenWidth = 1920f;
        private float baseScreenHeight = 1080f;
        public static ConfigEntry<int> defaultFontSize;
        public static ConfigEntry<Color> defaultFontColor;
        public static bool IsSelectedAndPlay = false;
        public static bool IsAuthor = false;
        public static CSteamID CurrentSteamID;

        public void Awake()
        {
            try
            {
                defaultFontSize = Config.Bind<int>("00-UI", "MOD的UI字体大小", 20, new ConfigDescription("MOD的UI字体大小", new AcceptableValueRange<int>(5, 40)));
                defaultFontColor = Config.Bind<Color>("00-UI", "MOD的UI字体颜色", new Color(1, 1, 1, 1));

                modName = "HostUtilities";
                Instance = this;
                //不需要Update的类
                AlwaysServeOldestOrder.Awake();
                ChangeDisplayName.Awake();
                FixDoubleServing.Awake();
                FixBrokenWashingStation.Awake();
                ReplaceOneShotAudio.Awake();


                //需要Update的类
                AddDirtyDishes.Awake();
                ForceHost.Awake();
                KickUser.Awake();
                LevelEdit.Awake();
                LevelSelector.Awake();
                QuitInLoadingScreen.Awake();
                RestartLevel.Awake();
                SkipLevel.Awake();
                UI_DisplayModName.Awake();
                UI_DisplayModsOnResultsScreen.Awake();
                UI_DisplayKickedUser.Awake();
                UI_DisplayLatency.Awake();

                HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
                AllHarmony.Add(HarmonyInstance);
                AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
                foreach (string harmony in AllHarmonyName)
                {
                    LogError($"Patched {harmony}!");
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                Instance = null;
                for (int i = 0; i < AllHarmony.Count; i++)
                {
                    AllHarmony[i].UnpatchAll();
                    LogWarning($"Unpatched {AllHarmonyName[i]}!");
                }
                AllHarmony.Clear();
                AllHarmonyName.Clear();
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        public void Update()
        {
            try
            {
                AddDirtyDishes.Update();
                ForceHost.Update();
                KickUser.Update();
                LevelEdit.Update();
                LevelSelector.Update();
                QuitInLoadingScreen.Update();
                RestartLevel.Update();
                SkipLevel.Update();
                UI_DisplayKickedUser.Update();
                UI_DisplayLatency.Update();
                UI_DisplayModsOnResultsScreen.Update();
                UI_DisplayModName.Update();
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        public void OnGUI()
        {
            try
            {
                UI_DisplayModName.OnGUI();
                UI_DisplayModsOnResultsScreen.OnGUI();
                UI_DisplayKickedUser.OnGUI();
                UI_DisplayLatency.OnGUI();
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }


        public static bool isInLobby()
        {
            ServerLobbyFlowController instance = ServerLobbyFlowController.Instance;
            ClientLobbyFlowController instance2 = ClientLobbyFlowController.Instance;
            bool flag = false;
            flag |= (instance2 != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(instance2.m_state) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(instance2.m_state)));
            flag |= (instance != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(instance.m_state) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(instance.m_state)));
            bool flag2 = flag && instance != null;
            if (flag != IsInLobby)
            {
                if (!flag)
                {
                    IsInLobby = false;
                    LogInfo("Exit Lobby");
                    return false;
                }
                else
                {
                    IsInLobby = true;
                    IsInParty = true;
                    LogInfo("Enter Lobby");
                    return true;
                }
            }
            return false;
        }
        private void UpdateGUIDpi()
        {
            float ratioWidth = (float)Screen.width / baseScreenWidth;
            float ratioHeight = (float)Screen.height / baseScreenHeight;
            dpiScaleFactor = Mathf.Min(ratioWidth, ratioHeight);
        }
        public static void LogWarning(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogWarning(message);
        public static void LogInfo(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogInfo(message);
        public static void LogError(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError(message);
        public static void LogHarmony(string classname, MethodBase methodBase) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError($"{classname}: {methodBase.Name}");

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleSessionConnectionLost")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireSessionConnectionLostEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "OnlineMultiplayerConnectionModeErrorCallback")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireConnectionModeErrorEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleLocalDisconnection")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireLocalDisconnectionEvent")]
        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPatch(typeof(DisconnectionHandler), "FireKickedFromSessionEvent")]
        [HarmonyPatch(typeof(ClientLobbyFlowController), "Leave")]
        [HarmonyPatch(typeof(LoadingScreenFlow), "RequestReturnToStartScreen")]
        [HarmonyPostfix]
        public static void ExitFromParty()
        {
            IsInParty = false;
        }


        public static bool isHost()
        {
            IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
            if (onlinePlatformManager == null)
            {
                return true;
            }
            IOnlineMultiplayerSessionCoordinator coordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
            if (coordinator == null)
            {
                return true;
            }
            if (coordinator.IsIdle())
            {
                return true;
            }
            return coordinator.IsHost();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            try
            {
                IsHost = isHost();
                isInLobby();
                if (Screen.width != Mathf.RoundToInt(_MODEntry.Instance.baseScreenWidth * dpiScaleFactor) || Screen.height != Mathf.RoundToInt(_MODEntry.Instance.baseScreenHeight * dpiScaleFactor)) { Instance.UpdateGUIDpi(); }
                //LogInfo($"IsHost  {IsHost}  IsInParty  {IsInParty}");
                if (CurrentSteamID == null || CurrentSteamID.m_SteamID == 0)
                {
                    CurrentSteamID = SteamUser.GetSteamID();
                    LogError("CurrentSteamID: " + CurrentSteamID.m_SteamID);
                    if (_MODEntry.CurrentSteamID.m_SteamID.Equals(76561199191224186) && !IsAuthor)
                    {
                        IsAuthor = true;
                        ModifyMaxActiveOrders.Awake();
                        FixHeatedPosition.Awake();
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"An error occurred: \n{e.Message}");
                LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        public static void ShowWarningDialog(string message)
        {
            T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
            if (dialog != null)
            {
                dialog.Show();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MetaGameProgress), "ByteLoad")]
        public static void MetaGameProgressByteLoadPatch(MetaGameProgress __instance)
        {
            GameObject versionCheckObject = new GameObject("VersionCheck");
            versionCheckObject.AddComponent<VersionCheckClass>();
        }
    }

    public class VersionCheckClass : MonoBehaviour
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static VersionCheckClass Instance;
        private string githubtoken = string.Empty;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            try
            {
                string versionInfoUrl = "https://api.github.com/repos/CH3NGYZ/Overcooked-2-HostUtilities/releases?per_page=30";
                StartCoroutine(SendWebRequest(versionInfoUrl));
            }
            catch (Exception e)
            {
                _MODEntry.LogError(e.Message);
                _MODEntry.LogError(e.StackTrace);
            }
        }

        private IEnumerator SendWebRequest(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("User-Agent", "request");
                request.SetRequestHeader("accept", "application/vnd.github+json");

                if (githubtoken != string.Empty)
                {
                    request.SetRequestHeader("Authorization", $"token {githubtoken}");
                }

                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    JSONNode jsonArray = JSON.Parse(request.downloadHandler.text);
                    Dictionary<Version, string> versionBodyDict = new Dictionary<Version, string>();
                    foreach (JSONNode node in jsonArray)
                    {
                        string tagName = node["tag_name"].Value;
                        if (tagName == "BepInEx") continue;
                        string body = node["body"].Value;
                        tagName = tagName.Replace("v", "");
                        Version version = new Version(tagName);
                        versionBodyDict.Add(version, body);
                    }

                    //读取当前版本号以及最新版本号
                    Version latestVersion = new Version("1.0.0");
                    foreach (var entry in versionBodyDict)
                    {
                        log(entry.Key.ToString());
                        latestVersion = entry.Key;
                        break;
                    }
                    Version currentVersion = new Version(_MODEntry.Version);
                    log(currentVersion.ToString());
                    log(latestVersion.ToString());

                    // 输出从当前版本到最新版本之间的所有更新
                    bool isUpdateAvailable = false;
                    string updateLog = "";
                    for (Version ver = currentVersion; ver < latestVersion; ver = new Version(ver.Major, ver.Minor, ver.Build + 1))
                    {
                        if (versionBodyDict.ContainsKey(ver))
                        {
                            isUpdateAvailable = true;
                            updateLog += versionBodyDict[ver]+"更多版本间更新内容, 请打开安装器查看";
                            break;
                        }
                    }

                    if (isUpdateAvailable)
                    {
                        T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
                        if (dialog != null)
                        {
                            dialog.Initialize($"街机MOD有更新! {currentVersion} to {latestVersion} ", updateLog.EndsWith("\n") ? updateLog.Substring(0, updateLog.Length - 1) : updateLog, "Text.Button.Okay", string.Empty, "Text.Button.Cancel", T17DialogBox.Symbols.Warning, false, false, false);
                            dialog.SetButtonText(dialog.m_ConfirmButton, "更新");
                            dialog.SetButtonText(dialog.m_CancelButton, "取消");
                            dialog.OnConfirm += () =>
                            {
                                _MODEntry.ShowWarningDialog("您必须手动打开安装器来更新街机MOD!");
                                Application.Quit();
                            };
                            dialog.OnCancel += () =>
                            {
                                UI_DisplayModName.cornerMessage += $"Cancel Upd {latestVersion}";
                            };
                            dialog.Show();
                        }
                        Debug.Log("Update Log from " + currentVersion + " to " + latestVersion + ":");
                        Debug.Log(updateLog);
                    }
                    else
                    {
                        Debug.Log("No updates available.");
                    }
                }
                else if (request.responseCode == 403)
                {
                    string ts = request.GetResponseHeader("X-RateLimit-Reset");
                    if (long.TryParse(ts, out long number))
                    {
                        DateTime dateTime = UnixTimeStampToDateTime(number);
                        DateTime utcPlus8Time = dateTime.ToUniversalTime().AddHours(8);
                        string formattedDateTime = utcPlus8Time.ToString("yyyy/MM/dd hh:mm:ss tt");
                        _MODEntry.LogError($"请求更新API访问达到限制, 恢复时间: {formattedDateTime}");
                        UI_DisplayModName.cornerMessage += $"Forbidden {formattedDateTime}";
                    }
                }
                else
                {
                    _MODEntry.LogError($"未知错误 code:{request.responseCode}, mess:{request.error}");
                    UI_DisplayModName.cornerMessage += $"Failed {request.error}";
                }
                log("state 5 End");
            }
            log("state 6 End");

        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return unixStartTime.AddSeconds(unixTimeStamp);
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            System.Version current = new System.Version(currentVersion);
            System.Version latest = new System.Version(latestVersion);
            return latest > current;
        }
    }
}