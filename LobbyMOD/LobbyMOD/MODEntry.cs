using BepInEx;
using HarmonyLib;
using UnityEngine;
//dll文件输出路径更改: 本项目名LobbyMOD右键属性-生成-输出路径 改为你的游戏所在路径 Overcooked! 2/BepInEx/plugins/ 下
//生成dll后自动打开游戏测试: 本项目名LobbyMOD右键属性-生成事件-生成前/后  将gamePath替换成自己游戏的路径

namespace LobbyMODS
{
    [BepInPlugin("com.ch3ngyz.plugin.LobbyMods", "[街机主机MOD] By.酷茶 Q群860480677 本MOD完全免费", "1.0.25")]
    [BepInProcess("Overcooked2.exe")]
    public class MODEntry : BaseUnityPlugin
    {
        public static string modName;
        public static MODEntry Instance;
        public static bool IsInLobby;
        public static bool IsHost;
        public static bool IsInParty;
        public static float dpiScaleFactor = 1f;
        private float baseScreenWidth = 1920f;
        private float baseScreenHeight = 1080f;
        public void Awake()
        {
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
            //UnlockChefs.Awake();
            //UnlockDlcs.Awake();
            ReplaceOneShotAudio.Awake();
            ForceHost.Awake();
            Recipe.Awake();
            DisplayLatencyUI.Awake();
            FixDoubleServing.Awake();
            RestartLevel.Awake();



            Harmony.CreateAndPatchAll(typeof(MODEntry));
        }

        public void Update()
        {
            if (Screen.width != Mathf.RoundToInt(baseScreenWidth * dpiScaleFactor) || Screen.height != Mathf.RoundToInt(baseScreenHeight * dpiScaleFactor))
                UpdateGUIDpi();
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

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPostfix]
        public static void postfix1()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleSessionConnectionLost")]
        [HarmonyPostfix]
        public static void postfix2()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireSessionConnectionLostEvent")]
        [HarmonyPostfix]
        public static void postfix3()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "OnlineMultiplayerConnectionModeErrorCallback")]
        [HarmonyPostfix]
        public static void postfix4()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireConnectionModeErrorEvent")]
        [HarmonyPostfix]
        public static void postfix5()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleLocalDisconnection")]
        [HarmonyPostfix]
        public static void postfix6()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireLocalDisconnectionEvent")]
        [HarmonyPostfix]
        public static void postfix7()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPostfix]
        public static void postfix8()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireKickedFromSessionEvent")]
        [HarmonyPostfix]
        public static void postfix9()
        {
            IsInParty = false;
        }

        [HarmonyPatch(typeof(ClientLobbyFlowController), "Leave")]
        [HarmonyPrefix]
        public static bool prefix5()
        {
            IsInParty = false;
            return true;
        }

        [HarmonyPatch(typeof(LoadingScreenFlow), "RequestReturnToStartScreen")]
        [HarmonyPrefix]
        public static bool prefix6()
        {
            IsInParty = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            IsHost = ConnectionStatus.IsHost();
            isInLobby();
            LogInfo($"IsHost  {IsHost}  IsInParty  {IsInParty}");
        }
    }
}
