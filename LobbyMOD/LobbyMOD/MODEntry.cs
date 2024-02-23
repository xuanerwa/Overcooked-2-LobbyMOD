using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LobbyMODS.Recipe;
using Team17.Online;
//dll文件输出路径更改: 本项目名LobbyMOD右键属性-生成-输出路径 改为你的游戏所在路径 Overcooked! 2/BepInEx/plugins/ 下
//生成dll后自动打开游戏测试: 本项目名LobbyMOD右键属性-生成事件-生成前/后  将gamePath替换成自己游戏的路径

namespace LobbyMODS
{
    [BepInPlugin("com.ch3ngyz.plugin.LobbyMods", "[街机主机MOD] By.酷茶 Q群860480677", "1.0.21")]
    [BepInProcess("Overcooked2.exe")]
    public class MODEntry : BaseUnityPlugin
    {
        public static string modName;
        public static MODEntry Instance;
        public static bool IsInLobby;
        public static bool IsHost;
        public static bool playinlobby;
        public void Awake()
        {
            modName = "街机主机工具";
            Instance = this;
            IsInLobby = false;
            IsHost = false;
            DisplayModsOnResultsScreenUI.Awake();
            SkipLevel.Awake();
            LobbyKickUser.Awake();
            LobbyKevin.Awake();
            QuitWhenLoadScene.Awake();
            DisplayKickedUserUI.Awake();
            UnlockChefs.Awake();
            UnlockDlcs.Awake();
            ReplaceOneShotAudio.Awake();
            ForceHost.Awake();
            Recipe.Awake();
            DisplayLatencyUI.Awake();
            FixDoubleServing.Awake();
            Harmony.CreateAndPatchAll(typeof(MODEntry));
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
            IsHost = ConnectionStatus.IsHost();
            //LogError($"是否主机{IsHost}");
            isInLobby();
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
                    playinlobby = true;
                    LogInfo("Enter Lobby");
                    return true;
                }
            }
            return false;
        }

        public static void LogWarning(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogWarning(message);
        public static void LogInfo(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogInfo(message);
        public static void LogError(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError(message);

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPostfix]
        public static void postfix1()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleSessionConnectionLost")]
        [HarmonyPostfix]
        public static void postfix2()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireSessionConnectionLostEvent")]
        [HarmonyPostfix]
        public static void postfix3()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "OnlineMultiplayerConnectionModeErrorCallback")]
        [HarmonyPostfix]
        public static void postfix4()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireConnectionModeErrorEvent")]
        [HarmonyPostfix]
        public static void postfix5()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleLocalDisconnection")]
        [HarmonyPostfix]
        public static void postfix6()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireLocalDisconnectionEvent")]
        [HarmonyPostfix]
        public static void postfix7()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "HandleKickMessage")]
        [HarmonyPostfix]
        public static void postfix8()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(DisconnectionHandler), "FireKickedFromSessionEvent")]
        [HarmonyPostfix]
        public static void postfix9()
        {
            playinlobby = false;
        }

        [HarmonyPatch(typeof(ClientLobbyFlowController), "Leave")]
        [HarmonyPrefix]
        public static bool prefix5()
        {
            playinlobby = false;
            return true;
        }

        [HarmonyPatch(typeof(LoadingScreenFlow), "RequestReturnToStartScreen")]
        [HarmonyPrefix]
        public static bool prefix6()
        {
            playinlobby = false;
            return true;
        }
    }
}
