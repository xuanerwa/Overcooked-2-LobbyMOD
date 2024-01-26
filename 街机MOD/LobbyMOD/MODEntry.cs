using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//dll文件输出路径更改: 本项目名LobbyMOD右键属性-生成-输出路径 改为你的游戏所在路径 Overcooked! 2/BepInEx/plugins/ 下
//生成dll后自动打开游戏测试: 本项目名LobbyMOD右键属性-生成事件-生成前/后  将gamePath替换成自己游戏的路径

/*
 
修改完毕之后进入仓库文件夹(文件夹里有README.md)下运行:
git rm --cached  街机MOD/LobbyMOD/LobbyMOD.csproj
git commit -m "Remove and ignore LobbyMOD.csproj"

*/
namespace LobbyMODS
{
    [BepInPlugin("com.ch3ngyz.plugin.LobbyMods", "[街机MOD] By.酷茶 Q群860480677", "1.0.8")]
    [BepInProcess("Overcooked2.exe")]
    public class MODEntry : BaseUnityPlugin
    {
        public static string modName = "街机工具集合";
        public static MODEntry Instance;
        public void Awake()
        {
            Instance = this;
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
        public static bool IsInLobby = false;
        public static readonly AccessTools.FieldRef<ServerLobbyFlowController, LobbyFlowController.LobbyState> m_state_server = AccessTools.FieldRefAccess<ServerLobbyFlowController, LobbyFlowController.LobbyState>("m_state");
        public static readonly AccessTools.FieldRef<ClientLobbyFlowController, LobbyFlowController.LobbyState> m_state_client = AccessTools.FieldRefAccess<ClientLobbyFlowController, LobbyFlowController.LobbyState>("m_state");
        public static void LogWarning(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogWarning(message);
        public static void LogInfo(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogInfo(message);
        public static void LogError(string message) => BepInEx.Logging.Logger.CreateLogSource(modName).LogError(message);
    }
}
