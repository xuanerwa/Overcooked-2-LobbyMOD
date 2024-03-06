using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace LobbyMODS
{
    public class RestartLevel
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> restartLevel;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            restartLevel = MODEntry.Instance.Config.Bind("01-按键绑定", "12-一键重开", KeyCode.F11, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(restartLevel.Value))
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
            }
        }
    }
}
