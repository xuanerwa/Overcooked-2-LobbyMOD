using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Team17.Online;
using UnityEngine;
using UnityEngine.UI;
using static DataStore;
using static GameSession;
using static SceneDirectoryData;

namespace HostUtilities
{
    public class LevelSelector : MonoBehaviour
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static Dictionary<string, SceneDirectoryEntry> DirectoryDict = new Dictionary<string, SceneDirectoryEntry>();
        public static ConfigEntry<string> ValueList = null;
        public static Harmony HarmonyInstance { get; set; }
        public static ConfigEntry<KeyCode> startSelectedLevel;

        public static void Awake()
        {
            startSelectedLevel = _MODEntry.Instance.Config.Bind<KeyCode>("00-选关", "00-4秒后开始选定关卡", KeyCode.F5, "开始玩已选的关卡");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public static void Update()
        {

            if (Input.GetKeyDown(startSelectedLevel.Value))
            {

                if (!_MODEntry.IsHost)
                {
                    _MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                if (ValueList == null)
                {
                    _MODEntry.ShowWarningDialog("请至少以主机身份进入一次街机大厅并等待5秒(可以打开强制主机)");
                    return;
                }
                if (!_MODEntry.IsInLobby)
                {
                    _MODEntry.ShowWarningDialog("不在街机大厅，别点啦");
                    return;
                }
                Predicate<SceneDirectoryData.SceneDirectoryEntry> matchScene = (SceneDirectoryData.SceneDirectoryEntry entry) =>
                {
                    if (entry.Label.Contains("ThroneRoom") || entry.Label.Contains("TutorialLevel"))
                    {
                        return false;
                    }
                    if (entry.Label.Equals(GetSceneDirectoryEntryFromChinese(ValueList.Value).Label)) { return true; }
                    return false;
                };
                //log(ValueList.Value);
                //log(GetSceneDirectoryEntryFromChinese(ValueList.Value).Label);

                _MODEntry.IsSelectedAndPlay = true;
                ServerLobbyFlowController Instance = ServerLobbyFlowController.Instance;
                if (Instance != null)
                {
                    int player_index = 0;
                    while ((long)player_index < (long)((ulong)OnlineMultiplayerConfig.MaxPlayers))
                    {
                        Instance.SelectTheme(SceneDirectoryData.LevelTheme.Random, player_index);
                        LobbyFlowController.LobbyState? serverLobbyState = ServerLobbyFlowController.Instance.m_state;
                        LobbyFlowController.LobbyState lobbyState = LobbyFlowController.LobbyState.LocalThemeSelection;
                        if (serverLobbyState.GetValueOrDefault() == lobbyState & serverLobbyState != null)
                        {
                            ServerLobbyFlowController.Instance.SetState(LobbyFlowController.LobbyState.LocalThemeSelected);
                        }
                        else
                        {
                            serverLobbyState = ServerLobbyFlowController.Instance.m_state;
                            lobbyState = LobbyFlowController.LobbyState.OnlineThemeSelection;
                            if (serverLobbyState.GetValueOrDefault() == lobbyState & serverLobbyState != null)
                            {
                                ServerLobbyFlowController.Instance.SetState(LobbyFlowController.LobbyState.OnlineThemeSelected);
                            }
                        }
                        player_index++;
                    }

                    //ServerLobbyFlowController.Instance.ResetTimer(0);
                    //_MODEntry.ShowWarningDialog("4秒后自动开始选择的关卡, 请不要重复点击按键, 点击继续关闭");
                }
                FastList<SceneDirectoryData.SceneDirectoryEntry> fastList = new FastList<SceneDirectoryData.SceneDirectoryEntry>(60);
                SceneDirectoryData[] sceneDirectories = Instance.m_lobbyFlow.GetSceneDirectories();
                DLCManager dlcmanager = GameUtils.RequireManager<DLCManager>();
                List<DLCFrontendData> allDlc = dlcmanager.AllDlc;
                GameSession.GameType gameType = (!Instance.m_bIsCoop) ? GameSession.GameType.Competitive : GameSession.GameType.Cooperative;
                int[] array = new int[sceneDirectories.Length];
                for (int i = 0; i < sceneDirectories.Length; i++)
                {
                    DLCFrontendData dlcfrontendData = null;
                    int dlcidfromSceneDirIndex = Instance.m_lobbyFlow.GetDLCIDFromSceneDirIndex(gameType, i);
                    if (true)
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
                        fastList.AddRange(sceneDirectories[i].Scenes.FindAll(matchScene));
                    }
                    array[i] = fastList.Count;
                }
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
                SceneDirectoryData.SceneDirectoryEntry sceneDirectoryEntry = fastList._items[num];
                int dlcidfromSceneDirIndex2 = -1;
                try
                {
                    dlcidfromSceneDirIndex2 = Instance.m_lobbyFlow.GetDLCIDFromSceneDirIndex(gameType, idx);
                    log($"dlcidfromSceneDirIndex2: {dlcidfromSceneDirIndex2}");
                }
                catch (Exception e)
                {
                    log($"An error occurred: \n{e.Message}");
                    log($"Stack trace: \n{e.StackTrace}");
                }
                SceneDirectoryData.PerPlayerCountDirectoryEntry sceneVarient = sceneDirectoryEntry.GetSceneVarient(ServerUserSystem.m_Users.Count);
                if (sceneVarient == null)
                {
                    if (!Instance.m_bIsCoop)
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
                Instance.m_delayedLevelLoad = Instance.StartCoroutine(Instance.DelayedLevelLoad(sceneVarient.SceneName, dlcidfromSceneDirIndex2));
                log($"场景名 {sceneVarient.SceneName}  dlcidfromSceneDirIndex2:{dlcidfromSceneDirIndex2}");
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ClientDynamicLandscapeParenting), "Awake")]
        //public static void ClientDynamicLandscapeParenting_Awake_Patch(ClientDynamicLandscapeParenting __instance)
        //{
        //    log($"----------------------------------{GameUtils.GetLevelConfig()}");
        //}

        [HarmonyPatch(typeof(ServerLobbyFlowController), "PickLevel")]
        [HarmonyPrefix]
        private static bool ServerLobbyFlowController_PickLevel_Prefix(ref ServerLobbyFlowController __instance, SceneDirectoryData.LevelTheme _theme)
        {
            try
            {
                if (_MODEntry.IsSelectedAndPlay)
                {
                    log("已经开始指定关卡,不执行原开始关卡函数");
                    _MODEntry.IsSelectedAndPlay = false;
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        public static void ClientTime_OnTimeSyncReceived_Patch()
        {
            try
            {
                if (!_MODEntry.IsInLobby)
                {
                    return;
                }
                if (DirectoryDict.Count > 1)
                {
                    return;
                }
                DirectoryDict.Clear();

                List<SceneDirectoryEntry> levelList = GetLevelList();
                if (levelList.Count == 0)
                {
                    return;
                }
                //log(levelList.Count.ToString());
                for (int i = 0; i < levelList.Count; i++)
                {
                    SceneDirectoryEntry sceneDirectoryEntry = levelList[i];
                    DirectoryDict.Add(sceneDirectoryEntry.Label, sceneDirectoryEntry);
                    log($"I: {i + 1} L: {sceneDirectoryEntry.Label} N: {GetLevelName(sceneDirectoryEntry, false)}");
                }
                log("finished");

                List<string> strList = new List<string>();
                foreach (var pair in DirectoryDict)
                {
                    string LevelName = GetLevelName(DirectoryDict[pair.Key], false);
                    strList.Add(LevelName);
                }
                string[] strArray = strList.ToArray();
                ValueList = _MODEntry.Instance.Config.Bind<string>("00-选关", "选择关卡", strArray[0], new ConfigDescription("选择关卡", new AcceptableValueList<string>(strArray)));
                log(ValueList.Value);
            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
            }
        }
        public static SceneDirectoryData.SceneDirectoryEntry GetSceneDirectoryEntryFromChinese(string label)
        {
            foreach (var pair in DirectoryDict)
            {
                if (GetLevelName(DirectoryDict[pair.Key], false).Equals(label))
                {
                    return DirectoryDict[pair.Key];
                }
            }
            return null;
        }
        public static SceneDirectoryData[] GetSceneDirectories()
        {
            LobbyFlowController instance = LobbyFlowController.Instance;
            if (instance == null)
            {
                return null;
            }
            return instance.GetSceneDirectories();
        }

        public static List<SceneDirectoryEntry> GetLevelList()
        {

            ServerLobbyFlowController instance = ServerLobbyFlowController.Instance;
            LobbyFlowController instance2 = LobbyFlowController.Instance;
            if (instance == null)
            {
                return new List<SceneDirectoryEntry>();
            }
            if (instance2 == null)
            {
                return new List<SceneDirectoryEntry>();
            }
            SceneDirectoryData[] sceneDirectories = GetSceneDirectories();
            DLCManager dlcmanager = GameUtils.RequireManager<DLCManager>();
            if (sceneDirectories == null)
            {
                return new List<SceneDirectoryEntry>();
            }
            if (dlcmanager == null)
            {
                return new List<SceneDirectoryEntry>();
            }
            List<DLCFrontendData> allDlc = dlcmanager.AllDlc;
            GameSession.GameType gameType = (!instance.m_bIsCoop) ? GameSession.GameType.Competitive : GameSession.GameType.Cooperative;
            List<SceneDirectoryData.SceneDirectoryEntry> list = new List<SceneDirectoryEntry>();
            for (int i = 0; i < sceneDirectories.Length; i++)
            {
                DLCFrontendData dlcfrontendData = null;
                int dlcidfromSceneDirIndex = instance2.GetDLCIDFromSceneDirIndex(gameType, i);
                for (int j = 0; j < allDlc.Count; j++)
                {
                    DLCFrontendData dlcfrontendData2 = allDlc[j];
                    if (dlcfrontendData2.m_DLCID == dlcidfromSceneDirIndex)
                    {
                        dlcfrontendData = dlcfrontendData2;
                        break;
                    }
                }
                if (dlcfrontendData == null || dlcmanager.IsDLCAvailable(dlcfrontendData))
                {

                    list.AddRange(sceneDirectories[i].Scenes);
                }
            }
            list.RemoveAll((SceneDirectoryEntry x) => x.Label.Contains("ThroneRoom"));
            list.RemoveAll((SceneDirectoryEntry x) => x.Label.Contains("Tutorial"));
            //list.RemoveAll((SceneDirectoryEntry x) => x.Label.Contains("DLC07Battlements08"));
            return list;
        }
        public static string GetLevelName(SceneDirectoryData.SceneDirectoryEntry entry, bool withLevelLabel = false)
        {
            LobbyFlowController instance = LobbyFlowController.Instance;
            string levelname = GetLevelName(instance, entry, withLevelLabel);
            //log($"name {levelname} label {entry.Label}");
            if (levelname.Contains("节庆大餐") && entry.Label.Contains("DLC03"))
            {
                levelname = levelname.Replace("节庆大餐", "节庆 - 圣诞");
                //log("替换冬日");
            }
            if (levelname.Contains("节庆大餐") && entry.Label.Contains("DLC09"))
            {
                levelname = levelname.Replace("节庆大餐", "节庆 - 冬日");
                //log("替换圣诞");
            }
            if (levelname.Contains("王朝餐厅") && entry.Label.Contains("DLC04"))
            {
                levelname = levelname.Replace("王朝餐厅", "王朝 - 新年");
                //log("替换新年");
            }
            if (levelname.Contains("王朝餐厅") && entry.Label.Contains("DLC10"))
            {
                levelname = levelname.Replace("王朝餐厅", "王朝 - 春节");
                //log("替换冬日");
            }

            return levelname;
        }

        // Token: 0x0600006C RID: 108 RVA: 0x0000534C File Offset: 0x0000354C
        public static string GetLevelName(LobbyFlowController lobbyFlow, SceneDirectoryData.SceneDirectoryEntry entry, bool withLabel = false)
        {
            LevelNameInfo levelNameInfo = GetLevelNameInfo(lobbyFlow, entry);
            return levelNameInfo.ThemeName + " - " + levelNameInfo.LevelName + (withLabel ? string.Concat(new string[]
            {
                " (",
                levelNameInfo.ThemeLabel,
                " - ",
                levelNameInfo.LevelLabel,
                ")"
            }) : "");
        }
        public static LevelNameInfo GetLevelNameInfo(LobbyFlowController lobbyFlow, SceneDirectoryData.SceneDirectoryEntry entry)
        {
            string text;
            string themeName;
            if (lobbyFlow != null)
            {
                ThemeSelectButton buttonForTheme = lobbyFlow.m_themeSelectMenu.GetButtonForTheme(entry.Theme);
                if (buttonForTheme == null)
                {
                    return GetLevelNameInfo(null, entry);
                }
                T17Text componentInChildren = buttonForTheme.GetComponentInChildren<T17Text>(true);
                if (componentInChildren == null)
                {
                    return GetLevelNameInfo(null, entry);
                }
                text = componentInChildren.m_LocalizationTag;
                themeName = componentInChildren.text;
            }
            else
            {
                ThemeLabels.TryGetValue(entry.Theme, out text);
                if (text == null)
                {
                    text = "Other";
                    themeName = "Other";
                }
                else
                {
                    Localization.Get(text, out themeName, new LocToken[0]);
                }
            }
            string label = entry.Label;
            string levelName;
            Localization.Get(label, out levelName, new LocToken[0]);
            return new LevelNameInfo(themeName, levelName, text, label);
        }
        public static readonly Dictionary<SceneDirectoryData.LevelTheme, string> ThemeLabels = new Dictionary<SceneDirectoryData.LevelTheme, string>
        {
            {
                SceneDirectoryData.LevelTheme.Sushi,
                "Text.Theme.Sushi"
            },
            {
                SceneDirectoryData.LevelTheme.Balloon,
                "Text.Theme.Balloon"
            },
            {
                SceneDirectoryData.LevelTheme.Wizard,
                "Text.Theme.Wizard"
            },
            {
                SceneDirectoryData.LevelTheme.Space,
                "Text.Theme.Alien"
            },
            {
                SceneDirectoryData.LevelTheme.Rapids,
                "Text.Theme.Rapids"
            },
            {
                SceneDirectoryData.LevelTheme.Mine,
                "Text.Theme.Mine"
            },
            {
                SceneDirectoryData.LevelTheme.Random,
                "Text.Theme.Random"
            },
            {
                SceneDirectoryData.LevelTheme.Beach,
                "Text.Theme.Beach"
            },
            {
                SceneDirectoryData.LevelTheme.Resort,
                "Text.Theme.Resort"
            },
            {
                SceneDirectoryData.LevelTheme.Wonderland,
                "Text.Theme.Wonderland"
            },
            {
                SceneDirectoryData.LevelTheme.ChinaTown,
                "Text.Theme.Lunar"
            },
            {
                SceneDirectoryData.LevelTheme.Campsite,
                "Text.Theme.Campsite"
            },
            {
                SceneDirectoryData.LevelTheme.Treehouse,
                "Text.Theme.Treehouse"
            },
            {
                SceneDirectoryData.LevelTheme.Keep,
                "Text.Theme.Keep"
            },
            {
                SceneDirectoryData.LevelTheme.Courtyard,
                "Text.Theme.Courtyard"
            },
            {
                SceneDirectoryData.LevelTheme.Battlements,
                "Text.Theme.Battlements"
            },
            {
                SceneDirectoryData.LevelTheme.Outside,
                "Text.Theme.CircusGrounds"
            },
            {
                SceneDirectoryData.LevelTheme.Inside,
                "Text.Theme.Tent"
            },
            {
                SceneDirectoryData.LevelTheme.Wonderland2,
                "Text.Theme.DLC09Theme01"
            },
            {
                SceneDirectoryData.LevelTheme.ChinaTown2,
                "Text.Theme.Lunar2"
            },
            {
                SceneDirectoryData.LevelTheme.Summer,
                "Text.Theme.Summer"
            },
            {
                SceneDirectoryData.LevelTheme.ChinaTown3,
                "Text.Theme.MoonFestival"
            }
        };
    }
    public class LevelNameInfo
    {
        // Token: 0x06000259 RID: 601 RVA: 0x0000C817 File Offset: 0x0000AA17
        public LevelNameInfo(string themeName, string levelName, string themeLabel, string levelLabel)
        {
            ThemeName = themeName;
            LevelName = levelName;
            ThemeLabel = themeLabel;
            LevelLabel = levelLabel;
        }

        // Token: 0x0400014B RID: 331
        public readonly string ThemeName;

        // Token: 0x0400014C RID: 332
        public readonly string LevelName;

        // Token: 0x0400014D RID: 333
        public readonly string ThemeLabel;

        // Token: 0x0400014E RID: 334
        public readonly string LevelLabel;

    }
}
