using BepInEx.Configuration;
using InControl;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class RestartLevel
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<KeyCode> restartLevelKey;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            restartLevelKey = MODEntry.Instance.Config.Bind("02-按键绑定", "12-一键重开", KeyCode.F11, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(restartLevelKey.Value))
            {
                if (!MODEntry.isHost)
                {
                    MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                if (!cooling)
                {
                    Log("重启关卡");
                    RestartLevelMain();
                }
                else
                {
                    Log("重启关卡冷静期");
                }
                if (System.Environment.TickCount - startTime > 8000)
                {
                    cooling = false;
                    Log("重启关卡");
                    RestartLevelMain();
                }
            }
        }
        public static void RestartLevelMain()
        {
            string nextScene = GameUtils.GetGameSession().LevelSettings.SceneDirectoryVarientEntry.SceneName;
            if (!string.IsNullOrEmpty(nextScene))
            {
                GameState setAtLoadingBegin = GameState.LoadKitchen;
                GameState waitForHide = GameState.RunLevelIntro;
                bool bUseLoadingScreen = true;

                GameSession gameSession = GameUtils.GetGameSession();
                gameSession.FillShownMetaDialogStatus();
                ServerMessenger.GameProgressData(gameSession.Progress.SaveData, gameSession.m_shownMetaDialogs);
                ServerMessenger.LoadLevel(nextScene, setAtLoadingBegin, bUseLoadingScreen, waitForHide);
            }
            startTime = System.Environment.TickCount;
            cooling = true;
        }
    }
}
