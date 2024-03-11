using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
using UnityEngine;
namespace LobbyMODS
{
    [BepInPlugin("com.ch3ngyz.plugin.LobbyMods", "[街机主机MOD] By.酷茶 Q群164509805 本MOD完全免费", "1.0.32")]
    [BepInProcess("Overcooked2.exe")]
    public class MODEntry : BaseUnityPlugin
    {
        public static Harmony HarmonyInstance { get; set; }
        public static List<string> AllHarmonyName = new List<string>();
        public static List<Harmony> AllHarmony = new List<Harmony>();
        public static string modName;
        public static MODEntry Instance;
        public static bool IsInLobby;
        public static bool IsHost;
        public static bool IsInParty;
        public static float dpiScaleFactor = 1f;
        private float baseScreenWidth = 1920f;
        private float baseScreenHeight = 1080f;
        public static ConfigEntry<int> defaultFontSize;
        public static ConfigEntry<string> defaultFontColor;
        public void Awake()
        {
            defaultFontSize = Config.Bind<int>("00-UI", "MOD的UI字体大小", 20);
            defaultFontColor = Config.Bind<string>("00-UI", "MOD的UI字体颜色(#+6位字母数字组合)", "#FFFFFF");
            modName = "街机主机工具";
            Instance = this;
            IsInLobby = false;
            IsHost = false;
            ModifyConfig.Awake();
            DisplayModsOnResultsScreenUI.Awake();
            SkipLevel.Awake();
            LobbyKickUser.Awake();
            LobbyKevin.Awake();
            QuitWhenLoadScene.Awake();
            DisplayKickedUserUI.Awake();
            ReplaceOneShotAudio.Awake();
            ForceHost.Awake();
            Recipe.Awake();
            DisplayLatencyUI.Awake();
            FixDoubleServing.Awake();
            RestartLevel.Awake();
            ChangeDisplayName.Awake();
            //UnlockChefs.Awake();
            //UnlockDlcs.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
            foreach (string harmony in AllHarmonyName)
            {
                LogError($"Patched {harmony}!");
            }
        }

        private void OnDestroy()
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

        public void Update()
        {
            DisplayModsOnResultsScreenUI.Update();
            SkipLevel.Update();
            LobbyKickUser.Update();
            LobbyKevin.Update();
            QuitWhenLoadScene.Update();
            DisplayKickedUserUI.Update();
            DisplayLatencyUI.Update();
            ForceHost.Update();
            Recipe.Update();
            RestartLevel.Update();
        }

        public void OnGUI()
        {
            DisplayModsOnResultsScreenUI.OnGUI();
            DisplayKickedUserUI.OnGUI();
            DisplayLatencyUI.OnGUI();
            Recipe.OnGUI();
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
            IsHost = isHost();
            isInLobby();
            if (Screen.width != Mathf.RoundToInt(MODEntry.Instance.baseScreenWidth * dpiScaleFactor) || Screen.height != Mathf.RoundToInt(MODEntry.Instance.baseScreenHeight * dpiScaleFactor))
                MODEntry.Instance.UpdateGUIDpi();
            //LogInfo($"IsHost  {IsHost}  IsInParty  {IsInParty}");
        }
    }
}
