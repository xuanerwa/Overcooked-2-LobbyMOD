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
using System.IO;
using UnityEngine.Networking;
using System.Diagnostics;
namespace HostUtilities
{
    [BepInPlugin("com.ch3ngyz.plugin.HostUtilities", "[HostUtilities] By.yc阿哲 Q群860480677 点击下方“‧‧‧”展开", "1.0.70")]
    [BepInProcess("Overcooked2.exe")]
    public class _MODEntry : BaseUnityPlugin
    {
        public static string Version = "1.0.70";
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
                dialog.Initialize("Text.Warning", "\"" + message.Replace("\n\n", "\n") + "\"", "Text.Button.Continue", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, true, true, false);
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
        public static VersionCheckClass Instance;

        public void Awake()
        {
            Instance = this;
        }
        public void Start()
        {
            try
            {
                //_MODEntry.LogError("startCoroutine");
                string versionInfoUrl = "https://api.github.com/repos/CH3NGYZ/Overcooked-2-HostUtilities/releases/latest";
                StartCoroutine(SendWebRequest(versionInfoUrl));
                //_MODEntry.LogError("stopCoroutine");
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
                yield return request.SendWebRequest();

                //Dictionary<string, string> responseHeaders = request.GetResponseHeaders();
                //_MODEntry.LogInfo("All Response Headers:");
                //foreach (var header in responseHeaders)
                //{
                //    _MODEntry.LogInfo($"{header.Key}: {header.Value}");
                //}

                if (request.responseCode == 200)
                {
                    _MODEntry.LogInfo("Response: " + request.downloadHandler.text);
                    ReleaseInfo versionInfo = JsonUtility.FromJson<ReleaseInfo>(request.downloadHandler.text);
                    string latestVersion = versionInfo.tag_name.Replace("v", "");
                    string releaseNote = versionInfo.body;
                    _MODEntry.LogInfo($"version {latestVersion}");
                    _MODEntry.LogInfo($"release {releaseNote}");
                    if (IsNewVersionAvailable(_MODEntry.Version, latestVersion))
                    {
                        string ignoreVersion;
                        string ignoreFilePath = "ignoreVersion.txt";

                        if (File.Exists(ignoreFilePath))
                        {
                            try
                            {
                                string fileContent = File.ReadAllText(ignoreFilePath);
                                ignoreVersion = fileContent.Replace(" ", "").Replace("\n", "").Replace("\r", "");
                            }
                            catch (Exception e)
                            {
                                _MODEntry.LogError($"An error occurred while reading the file: {e.Message}");
                                ignoreVersion = null;
                            }
                        }
                        else
                        {
                            ignoreVersion = null;
                        }


                        if (ignoreVersion != null && ignoreVersion == latestVersion)
                        {
                            UI_DisplayModName.cornerMessage += $"Ignored Upd v{latestVersion}";
                            yield break;
                        }

                        T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
                        if (dialog != null)
                        {
                            dialog.Initialize("街机MOD有更新!", $"更新日志:{releaseNote}".Replace("\n\n", "\n").Replace("\"", "\'"), "Text.Button.Okay", "Text.Button.Discard", "Text.Button.Cancel", T17DialogBox.Symbols.Warning, false, false, false);
                            dialog.Show();
                            dialog.OnConfirm += () =>
                            {
                                if (File.Exists("厨房MOD安装器.exe"))
                                {
                                    Process.Start("厨房MOD安装器.exe");
                                }
                                else
                                {
                                    _MODEntry.ShowWarningDialog("厨房MOD安装器.exe 不存在, 请手动从厨房根目录打开更新.");
                                    _MODEntry.LogError("厨房MOD安装器.exe 不存在, 请手动从厨房根目录打开更新.");
                                    UI_DisplayModName.cornerMessage += $"Installer NotExist Upd v{latestVersion}";
                                }
                            };
                            dialog.OnCancel += () =>
                            {
                                _MODEntry.LogError("Cancel");
                                UI_DisplayModName.cornerMessage += $"Cancel Upd v{latestVersion}";
                            };
                            dialog.OnDecline += () =>
                            {
                                _MODEntry.LogError("Decline");
                                try
                                {
                                    UI_DisplayModName.cornerMessage += $"Ignored Upd v{latestVersion}";
                                    File.WriteAllText(ignoreFilePath, latestVersion);
                                }
                                catch (Exception e)
                                {
                                    _MODEntry.LogError($"An error occurred while writing the file: {e.Message}");
                                }
                            };
                        }
                    }
                    else
                    {
                        UI_DisplayModName.cornerMessage += "Latest";
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
            }
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

        [System.Serializable]
        public class ReleaseInfo
        {
            public string tag_name;
            public string body;
        }
    }

}
