using BepInEx.Configuration;
using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Team17.Online;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MB3_MeshBakerRoot.ZSortObjects;
using static SceneDirectoryData;
using static WindowsAccessibility;


namespace HostUtilities
{
    public class LevelEdit
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static Harmony HarmonyInstance { get; set; }
        private static ConfigEntry<KeyCode> resetTimer;
        private static ConfigEntry<KeyCode> PlayRandom;
        public static ConfigEntry<bool> kevinEnabled;
        private static bool onlyKevin_;
        private static bool notKevin_;


        public static bool doOrigin = false;


        public static void Awake()
        {
            MServerLobbyFlowController.CreateConfigEntries();
            PlayRandom = MODEntry.Instance.Config.Bind<KeyCode>("02-按键绑定", "08-大厅计时器归零", KeyCode.Alpha6, "4秒后直接开始随机关卡");
            resetTimer = MODEntry.Instance.Config.Bind<KeyCode>("02-按键绑定", "09-大厅计时器45秒", KeyCode.Alpha7, "重置街机大厅时间为45秒");
            kevinEnabled = MODEntry.Instance.Config.Bind<bool>("04-修改关卡", "00-开启关卡自定义功能", false);
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        public static void Update()
        {
            if (Input.GetKeyDown(PlayRandom.Value))
            {
                if (!MODEntry.isHost)
                {
                    MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                MServerLobbyFlowController.ResetServerLobbyTimer(0.1f);
            }
            else if (Input.GetKeyDown(resetTimer.Value))
            {
                if (!MODEntry.isHost)
                {
                    MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                Log("重置街机大厅时间");
                MServerLobbyFlowController.ResetServerLobbyTimer(45f);
            }
            EnsureMutuallyExclusiveOptions();
        }


        static void EnsureMutuallyExclusiveOptions()
        {
            // 读取当前的选项值
            bool currentOnlyKevin = MServerLobbyFlowController.sceneDisableConfigEntries["02-只玩凯文和小节关"].Value;
            bool currentNotKevin = MServerLobbyFlowController.sceneDisableConfigEntries["01-不玩凯文和小节关"].Value;

            // 凯文2选1逻辑
            if (currentOnlyKevin && currentNotKevin)
            {
                // 检查并设置互斥逻辑
                if (currentOnlyKevin != onlyKevin_)
                {
                    MServerLobbyFlowController.sceneDisableConfigEntries["02-只玩凯文和小节关"].Value = true;
                    MServerLobbyFlowController.sceneDisableConfigEntries["01-不玩凯文和小节关"].Value = false;
                }
                else if (currentNotKevin != notKevin_)
                {
                    MServerLobbyFlowController.sceneDisableConfigEntries["01-不玩凯文和小节关"].Value = true;
                    MServerLobbyFlowController.sceneDisableConfigEntries["02-只玩凯文和小节关"].Value = false;
                }
            }
            // 更新缓存的值
            onlyKevin_ = MServerLobbyFlowController.sceneDisableConfigEntries["02-只玩凯文和小节关"].Value;
            notKevin_ = MServerLobbyFlowController.sceneDisableConfigEntries["01-不玩凯文和小节关"].Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            if (MServerLobbyFlowController.sceneDisableConfigEntries["01-不玩凯文和小节关"].Value == true)
            {
                //01-不玩凯文时以下所有选项自动关闭
                MServerLobbyFlowController.sceneDisableConfigEntries["01-关闭小节关"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["02-关闭主线凯文"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["03-关闭海滩凯文"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["04-关闭完美露营地凯文"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["05-关闭恐怖地宫凯文"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["06-关闭翻滚帐篷凯文"].Value = false;
                MServerLobbyFlowController.sceneDisableConfigEntries["07-关闭咸咸马戏团凯文"].Value = false;
            }
        }
    }

    public class MServerLobbyFlowController
    {
        private static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        private static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        private static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);

        public static Dictionary<string, ConfigEntry<bool>> sceneDisableConfigEntries = new Dictionary<string, ConfigEntry<bool>>();
        //private static Dictionary<int, Dictionary<SceneDirectoryEntry, bool>> alreadyPlayedSceneDict = new Dictionary<int, Dictionary<SceneDirectoryEntry, bool>>();
        private static Dictionary<string, SceneDirectoryEntryStatus> alreadyPlayedRandomSet = new Dictionary<string, SceneDirectoryEntryStatus>();
        private static Dictionary<string, SceneDirectoryEntryStatus> alreadyPlayedSceneSet = new Dictionary<string, SceneDirectoryEntryStatus>();

        public class SceneDirectoryEntryStatus
        {
            public SceneDirectoryEntry Entry { get; set; }
            public bool Status { get; set; }
        }

        public static void CreateConfigEntries()
        {
            CreateConfigEntry("04-修改关卡", "01-不玩凯文和小节关", true, "此选项打开时,04-禁用主题(凯文)下的所有开关,会被自动关闭.");
            CreateConfigEntry("04-修改关卡", "02-只玩凯文和小节关", false, "此选项打开时,04-禁用主题(非凯文)下的所有选项会失效,因为不玩普通关卡了.");
            CreateConfigEntry("04-禁用主题(凯文)", "01-关闭小节关");
            CreateConfigEntry("04-禁用主题(凯文)", "02-关闭主线凯文");
            CreateConfigEntry("04-禁用主题(凯文)", "03-关闭海滩凯文");
            CreateConfigEntry("04-禁用主题(凯文)", "04-关闭完美露营地凯文");
            CreateConfigEntry("04-禁用主题(凯文)", "05-关闭恐怖地宫凯文");
            CreateConfigEntry("04-禁用主题(凯文)", "06-关闭翻滚帐篷凯文");
            CreateConfigEntry("04-禁用主题(凯文)", "07-关闭咸咸马戏团凯文");
            CreateConfigEntry("04-禁用主题(非凯文)", "01-关闭世界1");
            CreateConfigEntry("04-禁用主题(非凯文)", "02-关闭世界2");
            CreateConfigEntry("04-禁用主题(非凯文)", "03-关闭世界3");
            CreateConfigEntry("04-禁用主题(非凯文)", "04-关闭世界4");
            CreateConfigEntry("04-禁用主题(非凯文)", "05-关闭世界5");
            CreateConfigEntry("04-禁用主题(非凯文)", "06-关闭世界6");
            CreateConfigEntry("04-禁用主题(非凯文)", "07-关闭节庆大餐");
            CreateConfigEntry("04-禁用主题(非凯文)", "08-关闭王朝餐厅");
            CreateConfigEntry("04-禁用主题(非凯文)", "09-关闭桃子游行");
            CreateConfigEntry("04-禁用主题(非凯文)", "10-关闭幸运灯笼");
            CreateConfigEntry("04-禁用主题(非凯文)", "11-关闭海滩");
            CreateConfigEntry("04-禁用主题(非凯文)", "12-关闭烧烤度假村");
            CreateConfigEntry("04-禁用主题(非凯文)", "13-关闭完美露营地");
            CreateConfigEntry("04-禁用主题(非凯文)", "14-关闭美味树屋");
            CreateConfigEntry("04-禁用主题(非凯文)", "15-关闭恐怖地宫");
            CreateConfigEntry("04-禁用主题(非凯文)", "16-关闭惊悚庭院");
            CreateConfigEntry("04-禁用主题(非凯文)", "17-关闭凶残城垛");
            CreateConfigEntry("04-禁用主题(非凯文)", "18-关闭翻滚帐篷");
            CreateConfigEntry("04-禁用主题(非凯文)", "19-关闭咸咸马戏团");

            CreateConfigEntry("04-修改关卡", "03-街机关卡不重复", true);
        }
        private static ConfigEntry<bool> configEntry;
        private static void CreateConfigEntry(string cls, string key)
        {
            configEntry = MODEntry.Instance.Config.Bind(cls, key, false);
            sceneDisableConfigEntries.Add(key, configEntry);
        }
        private static void CreateConfigEntry(string cls, string key, bool init)
        {
            configEntry = MODEntry.Instance.Config.Bind(cls, key, init);
            sceneDisableConfigEntries.Add(key, configEntry);
        }
        private static void CreateConfigEntry(string cls, string key, bool init, string desc)
        {
            configEntry = MODEntry.Instance.Config.Bind(cls, key, init, desc);
            sceneDisableConfigEntries.Add(key, configEntry);
        }


        public static void ResetServerLobbyTimer(float time = 45f)
        {
            ServerLobbyFlowController.Instance.ResetTimer(time);
        }

        public void PickRandom()
        {
            ServerLobbyFlowController.Instance.PickLevel(LevelTheme.Random);
        }


        public static void MPickLevel(ServerLobbyFlowController __instance, LevelTheme _theme)
        {
            Log($"开始关卡编辑逻辑, 选择的世界是{_theme}");
            PickLevelEdit(__instance, _theme, 197);
            return;
        }

        private static void PickLevelEdit(ServerLobbyFlowController __instance, LevelTheme theme, int line)
        {
            PickLevelEdit(__instance, theme);
            return;
        }

        private static void PickLevelEdit(ServerLobbyFlowController __instance, LevelTheme theme)
        {
            bool matchEdit(SceneDirectoryEntry entry)
            {

                if (entry.Label.Contains("ThroneRoom") || entry.Label.Contains("TutorialLevel"))
                {
                    return false;
                }
                bool condition90 = true;
                if (sceneDisableConfigEntries["01-不玩凯文和小节关"].Value == true)
                {
                    condition90 = entry.AvailableInLobby;
                }
                if (sceneDisableConfigEntries["02-只玩凯文和小节关"].Value == true)
                {
                    condition90 = !entry.AvailableInLobby;
                }
                bool condition100 = (entry.Theme == theme || theme == SceneDirectoryData.LevelTheme.Random);
                bool condition1 = !sceneDisableConfigEntries["01-关闭世界1"].Value || !(entry.World == SceneDirectoryData.World.One && entry.AvailableInLobby);
                bool condition2 = !sceneDisableConfigEntries["02-关闭世界2"].Value || !(entry.World == SceneDirectoryData.World.Two && entry.AvailableInLobby);
                bool condition3 = !sceneDisableConfigEntries["03-关闭世界3"].Value || !(entry.World == SceneDirectoryData.World.Three && entry.AvailableInLobby);
                bool condition4 = !sceneDisableConfigEntries["04-关闭世界4"].Value || !(entry.World == SceneDirectoryData.World.Four && entry.AvailableInLobby);
                bool condition5 = !sceneDisableConfigEntries["05-关闭世界5"].Value || !(entry.World == SceneDirectoryData.World.Five && entry.AvailableInLobby);
                bool condition6 = !sceneDisableConfigEntries["06-关闭世界6"].Value || !(entry.World == SceneDirectoryData.World.Six && entry.AvailableInLobby);
                bool condition7 = !sceneDisableConfigEntries["07-关闭节庆大餐"].Value || !(entry.World == SceneDirectoryData.World.DLC3_One);
                bool condition8 = !sceneDisableConfigEntries["07-关闭节庆大餐"].Value || !(entry.World == SceneDirectoryData.World.DLC9_One);
                bool condition9 = !sceneDisableConfigEntries["08-关闭王朝餐厅"].Value || !(entry.World == SceneDirectoryData.World.DLC4_One);
                bool condition10 = !sceneDisableConfigEntries["08-关闭王朝餐厅"].Value || !(entry.World == SceneDirectoryData.World.DLC10_One);
                bool condition11 = !sceneDisableConfigEntries["09-关闭桃子游行"].Value || !(entry.World == SceneDirectoryData.World.DLC11_One);
                bool condition12 = !sceneDisableConfigEntries["10-关闭幸运灯笼"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.ChinaTown3);
                bool condition13 = !sceneDisableConfigEntries["11-关闭海滩"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Beach);
                bool condition14 = !sceneDisableConfigEntries["12-关闭烧烤度假村"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Resort);
                bool condition15 = !sceneDisableConfigEntries["13-关闭完美露营地"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Campsite && entry.AvailableInLobby);
                bool condition16 = !sceneDisableConfigEntries["14-关闭美味树屋"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Treehouse);
                bool condition17 = !sceneDisableConfigEntries["15-关闭恐怖地宫"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Keep && entry.AvailableInLobby);
                bool condition18 = !sceneDisableConfigEntries["16-关闭惊悚庭院"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Courtyard);
                bool condition19 = !sceneDisableConfigEntries["17-关闭凶残城垛"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Battlements);
                bool condition20 = !sceneDisableConfigEntries["18-关闭翻滚帐篷"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Inside && entry.AvailableInLobby);
                bool condition21 = !sceneDisableConfigEntries["19-关闭咸咸马戏团"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Outside && entry.AvailableInLobby);
                bool condition22 = !sceneDisableConfigEntries["01-关闭小节关"].Value || !(entry.World == SceneDirectoryData.World.One || entry.World == SceneDirectoryData.World.Two || entry.World == SceneDirectoryData.World.Three || entry.World == SceneDirectoryData.World.Four || entry.World == SceneDirectoryData.World.Five || entry.World == SceneDirectoryData.World.Six);
                bool condition23 = !sceneDisableConfigEntries["02-关闭主线凯文"].Value || !(entry.World == SceneDirectoryData.World.Seven);
                bool condition24 = !sceneDisableConfigEntries["03-关闭海滩凯文"].Value || !entry.Label.Contains("DLC02HiddenLevel");
                bool condition25 = !sceneDisableConfigEntries["04-关闭完美露营地凯文"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Campsite && !entry.AvailableInLobby);
                bool condition26 = !sceneDisableConfigEntries["05-关闭恐怖地宫凯文"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Keep && !entry.AvailableInLobby);
                bool condition27 = !sceneDisableConfigEntries["06-关闭翻滚帐篷凯文"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Inside && !entry.AvailableInLobby);
                bool condition28 = !sceneDisableConfigEntries["07-关闭咸咸马戏团凯文"].Value || !(entry.Theme == SceneDirectoryData.LevelTheme.Outside && !entry.AvailableInLobby);
                bool shouldIncludeEntry = condition1 && condition2 && condition3 && condition4 && condition5 &&
                      condition6 && condition7 && condition8 && condition9 && condition10 && condition11 && condition12 &&
                      condition13 && condition14 && condition15 && condition16 && condition17 && condition18 && condition19 && condition20 &&
                      condition21 && condition22 && condition23 && condition24 && condition25 && condition26 && condition27 && condition28 &&
                      condition90 && condition100;

                return shouldIncludeEntry;
            }

            FastList<SceneDirectoryEntry> fastList = new FastList<SceneDirectoryEntry>(60);
            SceneDirectoryData[] sceneDirectories = __instance.m_lobbyFlow.GetSceneDirectories();


            DLCManager dlcmanager = GameUtils.RequireManager<DLCManager>();
            List<DLCFrontendData> allDlc = dlcmanager.AllDlc;
            GameSession.GameType gameType = (!__instance.m_bIsCoop) ? GameSession.GameType.Competitive : GameSession.GameType.Cooperative;
            int[] array = new int[sceneDirectories.Length];


            if (alreadyPlayedSceneSet.Count() == 0 && alreadyPlayedRandomSet.Count() == 0)
            {
                //Log("初始化已玩关卡");
                for (int i = 0; i < sceneDirectories.Length; i++)
                {
                    Dictionary<SceneDirectoryEntry, bool> alreadyPlayedSet = new Dictionary<SceneDirectoryEntry, bool>();
                    DLCFrontendData dlcfrontendData = null;
                    int dlcidfromSceneDirIndex = __instance.m_lobbyFlow.GetDLCIDFromSceneDirIndex(gameType, i);
                    if (theme == LevelTheme.Random)
                    {
                        for (int j = 0; j < allDlc.Count; j++)
                        {
                            DLCFrontendData dlcfrontendData2 = allDlc[j];
                            if (dlcfrontendData2.m_DLCID == dlcidfromSceneDirIndex)
                            {
                                dlcfrontendData = dlcfrontendData2;
                                break;
                            }
                        }
                    }
                    if (dlcfrontendData == null || dlcmanager.IsDLCAvailable(dlcfrontendData))
                    {
                        foreach (var Scenes in sceneDirectories[i].Scenes)
                        {
                            if (Scenes.Label.Contains("ThroneRoom") || Scenes.Label.Contains("TutorialLevel"))
                            {
                                continue;
                            }

                            if (dlcfrontendData == null || dlcmanager.IsDLCAvailable(dlcfrontendData))
                            {
                                alreadyPlayedSet.Add(Scenes, false);
                                alreadyPlayedSceneSet[Scenes.Label] = new SceneDirectoryEntryStatus { Entry = Scenes, Status = false };
                                alreadyPlayedRandomSet[Scenes.Label] = new SceneDirectoryEntryStatus { Entry = Scenes, Status = false };
                                //LogW($"初始化已玩关卡 {LevelSelector.GetLevelName(Scenes)} {Scenes.Label} {alreadyPlayedSceneSet[Scenes.Label].Status}");
                            }
                        }
                    }
                }
            }

            //var iddddd = 0;
            //foreach (var item in alreadyPlayedRandomSet)
            //{
            //    Log($"记录已玩随机关卡数量  index {iddddd}      content {item.Key}      Theme Random      status {item.Value.Status}      name {LevelSelector.GetLevelName(item.Value.Entry)}");
            //    iddddd += 1;
            //}
            //Log($"记录已玩随机关卡数量  {iddddd}");
            //iddddd = 0;
            //foreach (var item in alreadyPlayedRandomSet)
            //{
            //    Log($"记录已玩指定关卡数量  index {iddddd}      content {item.Key}      Theme {item.Value.Entry.Theme}      status {item.Value.Status} name {LevelSelector.GetLevelName(item.Value.Entry)}");
            //    iddddd += 1;
            //}
            //Log($"记录已玩指定关卡数量  {iddddd}");

            SceneDirectoryEntry[] matchedAllScene = new SceneDirectoryEntry[0];
            for (int i = 0; i < sceneDirectories.Length; i++)
            {
                DLCFrontendData dlcfrontendData = null;
                int dlcidfromSceneDirIndex = __instance.m_lobbyFlow.GetDLCIDFromSceneDirIndex(gameType, i);
                if (theme == LevelTheme.Random)
                {
                    for (int j = 0; j < allDlc.Count; j++)
                    {
                        DLCFrontendData dlcfrontendData2 = allDlc[j];
                        if (dlcfrontendData2.m_DLCID == dlcidfromSceneDirIndex)
                        {
                            dlcfrontendData = dlcfrontendData2;
                            break;
                        }
                    }
                }
                if (dlcfrontendData == null || dlcmanager.IsDLCAvailable(dlcfrontendData))
                {
                    SceneDirectoryEntry[] array2 = sceneDirectories[i].Scenes.FindAll(matchEdit);
                    //将matched的关卡记录到matchedAllScene变量中以供后续判断是不是没有match, 吧开关都关了
                    matchedAllScene = matchedAllScene.Concat(array2).ToArray();
                    foreach (SceneDirectoryEntry sceneDirectoryEntry1 in array2)
                    {
                        //-----------------去除待选关卡列表中的已玩关卡  开始
                        if (sceneDisableConfigEntries["03-街机关卡不重复"].Value == true)
                        {
                            if (theme == LevelTheme.Random)
                            {
                                if (alreadyPlayedRandomSet.ContainsKey(sceneDirectoryEntry1.Label) && alreadyPlayedRandomSet[sceneDirectoryEntry1.Label].Status == true)
                                {
                                    Log($"Repeated  L:{sceneDirectoryEntry1.Label}  N:{LevelSelector.GetLevelName(sceneDirectoryEntry1)}  T:{theme}");
                                    continue;
                                }
                            }
                            else
                            {
                                if (alreadyPlayedSceneSet.ContainsKey(sceneDirectoryEntry1.Label) && alreadyPlayedSceneSet[sceneDirectoryEntry1.Label].Status == true)
                                {
                                    Log($"Repeated  L:{sceneDirectoryEntry1.Label}  N:{LevelSelector.GetLevelName(sceneDirectoryEntry1)}  T:{theme}");
                                    continue;
                                }
                            }
                        }
                        //-----------------去除待选关卡列表中的已玩关卡 结束
                        fastList.Add(sceneDirectoryEntry1);
                    }
                }
                array[i] = fastList.Count;
            }

            //--------------重置开关 开始
            if (matchedAllScene.Count() == 0)
            {
                if (theme == LevelTheme.Random)
                {
                    LogW("关闭了太多的开关, 没有符合条件的关卡");
                    sceneDisableConfigEntries["01-关闭世界1"].Value = false;
                    sceneDisableConfigEntries["02-关闭世界2"].Value = false;
                    sceneDisableConfigEntries["03-关闭世界3"].Value = false;
                    sceneDisableConfigEntries["04-关闭世界4"].Value = false;
                    sceneDisableConfigEntries["05-关闭世界5"].Value = false;
                    sceneDisableConfigEntries["06-关闭世界6"].Value = false;
                    sceneDisableConfigEntries["07-关闭节庆大餐"].Value = false;
                    sceneDisableConfigEntries["07-关闭节庆大餐"].Value = false;
                    sceneDisableConfigEntries["08-关闭王朝餐厅"].Value = false;
                    sceneDisableConfigEntries["08-关闭王朝餐厅"].Value = false;
                    sceneDisableConfigEntries["09-关闭桃子游行"].Value = false;
                    sceneDisableConfigEntries["10-关闭幸运灯笼"].Value = false;
                    sceneDisableConfigEntries["11-关闭海滩"].Value = false;
                    sceneDisableConfigEntries["12-关闭烧烤度假村"].Value = false;
                    sceneDisableConfigEntries["13-关闭完美露营地"].Value = false;
                    sceneDisableConfigEntries["14-关闭美味树屋"].Value = false;
                    sceneDisableConfigEntries["15-关闭恐怖地宫"].Value = false;
                    sceneDisableConfigEntries["16-关闭惊悚庭院"].Value = false;
                    sceneDisableConfigEntries["17-关闭凶残城垛"].Value = false;
                    sceneDisableConfigEntries["18-关闭翻滚帐篷"].Value = false;
                    sceneDisableConfigEntries["19-关闭咸咸马戏团"].Value = false;
                    sceneDisableConfigEntries["01-关闭小节关"].Value = false;
                    sceneDisableConfigEntries["02-关闭主线凯文"].Value = false;
                    sceneDisableConfigEntries["03-关闭海滩凯文"].Value = false;
                    sceneDisableConfigEntries["04-关闭完美露营地凯文"].Value = false;
                    sceneDisableConfigEntries["05-关闭恐怖地宫凯文"].Value = false;
                    sceneDisableConfigEntries["06-关闭翻滚帐篷凯文"].Value = false;
                    sceneDisableConfigEntries["07-关闭咸咸马戏团凯文"].Value = false;
                    MODEntry.ShowWarningDialog("没有符合的关卡, 已重置关卡编辑的开关, 请重新自定义!");
                    PickLevelEdit(__instance, LevelTheme.Random, 467);
                    return;
                }
                else
                {
                    LogW("没有符合的关卡, 随机所有关卡");
                    PickLevelEdit(__instance, LevelTheme.Random, 420);
                    return;
                }
            }
            //--------------重置开关 结束

            int num = UnityEngine.Random.Range(0, fastList.Count);
            int idx = -1;
            for (int k = 0; k < array.Length; k++)
            {
                if (num < array[k])
                {
                    idx = k;
                    break;
                }
            }

            //for (int i = 0; i < fastList.Count; i++)
            //{
            //    Log($"FastList  L:{fastList._items[i].Label}  N:{LevelSelector.GetLevelName(fastList._items[i])}  T:{theme}");
            //}

            SceneDirectoryEntry PickLevel = fastList._items[num];
            LogE($"PickLeve  L:{PickLevel.Label}  N:{LevelSelector.GetLevelName(PickLevel)}  T:{theme}");


            //---------------关卡不重复功能   记录已玩关卡   重置已玩列表
            if (sceneDisableConfigEntries["03-街机关卡不重复"].Value == true)
            {
                LogW("关卡不重复功能已打开, 记录已玩关卡");
                if (theme == LevelTheme.Random)
                {
                    if (alreadyPlayedRandomSet.ContainsKey(PickLevel.Label))
                    {
                        alreadyPlayedRandomSet[PickLevel.Label] = new SceneDirectoryEntryStatus { Entry = PickLevel, Status = true };
                        alreadyPlayedSceneSet[PickLevel.Label] = new SceneDirectoryEntryStatus { Entry = PickLevel, Status = true };
                        //Log($"RecrdRan  L:{PickLevel.Label}  N:{LevelSelector.GetLevelName(PickLevel)}  S:{alreadyPlayedRandomSet[PickLevel.Label].Status}  T:{theme}");
                    }

                    //--------------------------重置已玩随机主题的关卡开始
                    // 是否应该重置随机已玩列表
                    bool shouldEmptyRandomList = false;
                    shouldEmptyRandomList = alreadyPlayedRandomSet
                        .Where(kv => matchEdit(kv.Value.Entry))
                        .All(kv => kv.Value.Status);

                    //--------------------------输出alreadyPlayedRandomSet
                    //{
                    //    foreach (var kv in alreadyPlayedRandomSet
                    //        .Where(kv => matchEdit(kv.Value.Entry)))
                    //    {
                    //        LogW($"RandList  L:{kv.Value.Entry.Label}  N:{LevelSelector.GetLevelName(kv.Value.Entry)}  S:{kv.Value.Status}  T:{theme}");
                    //    }
                    //}


                    //--------------------------输出alreadyPlayedRandomSet
                    if (shouldEmptyRandomList)
                    {
                        LogE($"reset {theme} alreadyPlayedSet");
                        Dictionary<string, SceneDirectoryEntryStatus> tempEntries = new Dictionary<string, SceneDirectoryEntryStatus>();
                        foreach (var KVP in alreadyPlayedRandomSet)
                        {
                            var tempEntry = KVP.Value.Entry;
                            tempEntries[KVP.Key] = new SceneDirectoryEntryStatus { Entry = tempEntry, Status = false };
                        }
                        foreach (var KVP in tempEntries)
                        {
                            alreadyPlayedRandomSet[KVP.Key] = KVP.Value;
                        }
                    }
                    //--------------------------重置已玩随机主题的关卡结束

                }
                else
                {
                    if (alreadyPlayedSceneSet.ContainsKey(PickLevel.Label))
                    {
                        alreadyPlayedRandomSet[PickLevel.Label] = new SceneDirectoryEntryStatus { Entry = PickLevel, Status = true };
                        alreadyPlayedSceneSet[PickLevel.Label] = new SceneDirectoryEntryStatus { Entry = PickLevel, Status = true };
                        //Log($"RecrdScn  L:{PickLevel.Label}  N:{LevelSelector.GetLevelName(PickLevel)}  S:{alreadyPlayedRandomSet[PickLevel.Label].Status}  T:{theme}");
                    }

                    //--------------------------重置已玩某个主题的关卡开始
                    //是否应该重置主题已玩列表
                    bool shouldEmptySceneList = false;
                    shouldEmptySceneList = alreadyPlayedSceneSet
                        .Where(kv => matchEdit(kv.Value.Entry) && theme == kv.Value.Entry.Theme)
                        .All(kv => kv.Value.Status);
                    //--------------------------输出alreadyPlayedSceneSet
                    //{
                    //    foreach (var kv in alreadyPlayedSceneSet
                    //    .Where(kv => matchEdit(kv.Value.Entry) && theme == kv.Value.Entry.Theme))
                    //    {
                    //        LogW($"ScenList  L:{kv.Value.Entry.Label}  N:{LevelSelector.GetLevelName(kv.Value.Entry)}  S:{kv.Value.Status}  T:{theme}");
                    //    }
                    //}
                    //--------------------------输出alreadyPlayedSceneSet


                    if (shouldEmptySceneList)
                    {
                        LogE($"reset {theme} alreadyPlayedSet");
                        Dictionary<string, SceneDirectoryEntryStatus> tempEntries = new Dictionary<string, SceneDirectoryEntryStatus>();
                        foreach (var KVP in alreadyPlayedSceneSet)
                        {
                            if (theme == KVP.Value.Entry.Theme)
                            {
                                var tempEntry = KVP.Value.Entry;
                                tempEntries[KVP.Key] = new SceneDirectoryEntryStatus { Entry = tempEntry, Status = false }; ;
                            }
                        }
                        foreach (var KVP in tempEntries)
                        {
                            alreadyPlayedSceneSet[KVP.Key] = KVP.Value;
                        }
                    }
                    //--------------------------重置已玩某个主题的关卡结束
                }
            }
            //----------------关卡不重复功能 结束

            SceneDirectoryEntry sceneDirectoryEntry = PickLevel;
            int dlcidfromSceneDirIndex2 = __instance.m_lobbyFlow.GetDLCIDFromSceneDirIndex(gameType, idx);
            PerPlayerCountDirectoryEntry sceneVarient = sceneDirectoryEntry.GetSceneVarient(ServerUserSystem.m_Users.Count);
            if (sceneVarient == null)
            {
                if (!__instance.m_bIsCoop)
                {
                    T17DialogBox dialog = T17DialogBoxManager.GetDialog(false);
                    dialog.Initialize("Text.Versus.NotEnoughPlayers.Title", "Text.Versus.NotEnoughPlayers.Message", "Text.Button.Confirm", null, null, T17DialogBox.Symbols.Warning, true, true, false);
                    T17DialogBox t17DialogBox = dialog;
                    t17DialogBox.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(t17DialogBox.OnConfirm, new T17DialogBox.DialogEvent(delegate ()
                                {
                                    ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, delegate (IConnectionModeSwitchStatus _status)
                                    {
                                        if (_status.GetProgress() == eConnectionModeSwitchProgress.Complete)
                                        {
                                            ServerGameSetup.Mode = GameMode.OnlineKitchen;
                                            ServerMessenger.LoadLevel("StartScreen", GameState.MainMenu, true, GameState.NotSet);
                                        }
                                    });
                                }));
                    dialog.Show();
                }
                return;
            }
            __instance.m_delayedLevelLoad = __instance.StartCoroutine(__instance.DelayedLevelLoad(sceneVarient.SceneName, dlcidfromSceneDirIndex2));
            return;
        }
    }
}

