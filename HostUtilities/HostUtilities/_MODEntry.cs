using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Team17.Online;
using UnityEngine;

namespace HostUtilities
{
    [BepInPlugin("com.ch3ngyz.plugin.HostUtilities", "[HostUtilities] By.yc阿哲 Q群860480677 点击下方“‧‧‧”展开", "1.0.48")]
    [BepInProcess("Overcooked2.exe")]
    public class _MODEntry : BaseUnityPlugin
    {
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
        public void Awake()
        {
            defaultFontSize = Config.Bind<int>("00-UI", "MOD的UI字体大小", 20, new ConfigDescription("MOD的UI字体大小", new AcceptableValueRange<int>(5, 40)));
            defaultFontColor = Config.Bind<Color>("00-UI", "MOD的UI字体颜色", new Color(1, 1, 1, 1));


            modName = "HostUtilities";
            Instance = this;
            ModifySingleplayerChopTimeMultiplier.Awake();
            UI_DisplayModsOnResultsScreen.Awake();
            SkipLevel.Awake();
            KickUser.Awake();
            LevelEdit.Awake();
            QuitInLoadingScreen.Awake();
            UI_DisplayKickedUser.Awake();
            ReplaceOneShotAudio.Awake();
            ForceHost.Awake();
            Recipe.Awake();
            UI_DisplayLatency.Awake();
            FixDoubleServing.Awake();
            RestartLevel.Awake();
            ChangeDisplayName.Awake();
            AlwaysServeOldestOrder.Awake();
            LevelSelector.Awake();
            AddCleanDishes.Awake();
            ModifyMaxActiveOrders.Awake();
            FixHeatedPosition.Awake();
            FixBrokenWashingStation.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            AllHarmony.Add(HarmonyInstance);
            AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
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
            UI_DisplayModsOnResultsScreen.Update();
            SkipLevel.Update();
            KickUser.Update();
            LevelEdit.Update();
            QuitInLoadingScreen.Update();
            UI_DisplayKickedUser.Update();
            UI_DisplayLatency.Update();
            ForceHost.Update();
            Recipe.Update();
            RestartLevel.Update();
            LevelSelector.Update();
            AddCleanDishes.Update();
        }

        public void OnGUI()
        {
            UI_DisplayModsOnResultsScreen.OnGUI();
            UI_DisplayKickedUser.OnGUI();
            UI_DisplayLatency.OnGUI();
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
            if (Screen.width != Mathf.RoundToInt(_MODEntry.Instance.baseScreenWidth * dpiScaleFactor) || Screen.height != Mathf.RoundToInt(_MODEntry.Instance.baseScreenHeight * dpiScaleFactor)) { Instance.UpdateGUIDpi(); }

            LogInfo($"IsHost  {IsHost}  IsInParty  {IsInParty}");
        }

        public static void ShowWarningDialog(string message)
        {
            T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
            if (dialog != null)
            {
                dialog.Initialize("Text.Warning", "\"" + message + "\"", "Text.Button.Continue", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, true, true, false);
                dialog.Show();
            }
        }
    }
}
