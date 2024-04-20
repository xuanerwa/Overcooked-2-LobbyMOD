using BepInEx.Configuration;
using InControl;
using UnityEngine;

namespace HostUtilities
{
    public class RestartLevel
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> restartLevelKey;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            restartLevelKey = _MODEntry.Instance.Config.Bind("01-按键绑定", "12-一键重开", KeyCode.F11, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(restartLevelKey.Value))
            {
                if (!cooling)
                {
                    log("重启关卡");
                    RestartLevelMain();
                }
                else
                {
                    log("重启关卡冷静期");
                }
                if (System.Environment.TickCount - startTime > 8000)
                {
                    cooling = false;
                    log("重启关卡");
                    RestartLevelMain();
                }
            }
        }
        public static void RestartLevelMain()
        {
            if (!_MODEntry.IsHost)
            {
                _MODEntry.ShowWarningDialog("你不是主机玩家，别点啦");
                return;
            }
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
