using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//dll文件输出路径更改: 本项目名LobbyMOD右键属性-生成-输出路径 改为你的游戏所在路径 Overcooked! 2/BepInEx/plugins/ 下
//生成dll后自动打开游戏测试: 本项目名LobbyMOD右键属性-生成事件-生成前/后  将gamePath替换成自己游戏的路径

namespace LobbyMODS
{
    [BepInPlugin("com.ch3ngyz.plugin.LobbyMods", "[街机MOD] By.酷茶 Q群860480677", "1.0.8")]
    [BepInProcess("Overcooked2.exe")]
    public class MODEntry : BaseUnityPlugin
    {
        public static string modName;
        public static MODEntry Instance;
        public static bool IsInLobby;
        public void Awake()
        {
            modName = "街机工具集合";
            Instance = this;
            IsInLobby = false;
            DisplayModsOnResultsScreen.Awake();
            SkipLevel.Awake();
            LobbyKickUser.Awake();
            LobbyKevin.Awake();
            QuitWhenLoadScene.Awake();
            DisplayKickedUser.Awake();
            UnlockChefs.Awake();
            UnlockDlcs.Awake();
            ReplaceOneShotAudio.Awake();
            ForceHost.Awake();
            m_state_server = AccessTools.FieldRefAccess<ServerLobbyFlowController, LobbyFlowController.LobbyState>("m_state");
            m_state_client = AccessTools.FieldRefAccess<ClientLobbyFlowController, LobbyFlowController.LobbyState>("m_state");
        }

        public void Update()
        {
            DisplayModsOnResultsScreen.Update();
            SkipLevel.Update();
            LobbyKickUser.Update();
            LobbyKevin.Update();
            QuitWhenLoadScene.Update();
            DisplayKickedUser.Update();
            ForceHost.Update();
            isInLobby();
        }

        public void OnGUI()
        {
            DisplayModsOnResultsScreen.OnGUI();
            DisplayKickedUser.OnGUI();
        }


        public static bool isInLobby()
        {
            ServerLobbyFlowController instance = ServerLobbyFlowController.Instance;
            ClientLobbyFlowController instance2 = ClientLobbyFlowController.Instance;
            bool flag = false;
            flag |= (instance2 != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(m_state_client.Invoke(instance2)) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(m_state_client.Invoke(instance2))));
            flag |= (instance != null && (LobbyFlowController.LobbyState.OnlineThemeSelection.Equals(m_state_server.Invoke(instance)) || LobbyFlowController.LobbyState.LocalThemeSelection.Equals(m_state_server.Invoke(instance))));
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
                    LogInfo("Enter Lobby");
                    return true;
                }
            }
            return false;
        }

        public static AccessTools.FieldRef<ServerLobbyFlowController, LobbyFlowController.LobbyState> m_state_server;
        public static AccessTools.FieldRef<ClientLobbyFlowController, LobbyFlowController.LobbyState> m_state_client;
        public static void LogWarning(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogWarning(message);
        public static void LogInfo(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogInfo(message);
        public static void LogError(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError(message);
    }
}
