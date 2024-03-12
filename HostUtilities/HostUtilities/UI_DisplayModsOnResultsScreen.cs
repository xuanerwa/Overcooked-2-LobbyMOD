using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using BitStream;
using BepInEx.Configuration;
using System.Reflection;
using System;

namespace HostUtilities
{
    public class UI_DisplayModsOnResultsScreen
    {
        public static Harmony HarmonyInstance { get; set; }
        private static MyOnScreenDebugDisplay onScreenDebugDisplay;
        private static ModsDisplay modsDisplay = null;
        private static bool shouldDisplay = false;
        private static ConfigEntry<bool> isshow = null;

        public static void Awake()
        {
            isshow = _MODEntry.Instance.Config.Bind<bool>("00-UI", "05-关卡结束时显示自定义关卡状态", true);
            /* Setup */
            onScreenDebugDisplay = new MyOnScreenDebugDisplay();
            onScreenDebugDisplay.Awake();
            modsDisplay = new ModsDisplay();
            onScreenDebugDisplay.AddDisplay(modsDisplay);
            /* Inject Mod */
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public static void Update()
        {
            onScreenDebugDisplay.Update();
        }

        public static void OnGUI()
        {
            onScreenDebugDisplay.OnGUI();
        }

        /* Adapted from OnScreenDebugDisplay */
        private class MyOnScreenDebugDisplay
        {
            public void AddDisplay(DebugDisplay display)
            {
                if (display != null)
                {
                    display.OnSetUp();
                    this.m_Displays.Add(display);
                }
            }

            public void RemoveDisplay(DebugDisplay display)
            {
                if (display != null)
                {
                    this.m_Displays.Remove(display);
                }
            }

            public void Awake()
            {
                this.m_Displays = new List<DebugDisplay>();
                this.m_GUIStyle = new GUIStyle();
                this.m_GUIStyle.alignment = TextAnchor.UpperRight;
                this.m_GUIStyle.fontSize = _MODEntry.defaultFontSize.Value;
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(_MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
            }

            public void Update()
            {
                for (int i = 0; i < this.m_Displays.Count; i++)
                {
                    this.m_Displays[i].OnUpdate();
                }
            }

            public void OnGUI()
            {
                m_GUIStyle.fontSize = Mathf.RoundToInt(_MODEntry.defaultFontSize.Value * _MODEntry.dpiScaleFactor);
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(_MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
                Rect rect = new Rect(0f, (float)Screen.height * 0.14f, (float)Screen.width * 0.99f, (float)this.m_GUIStyle.fontSize);
                for (int i = 0; i < this.m_Displays.Count; i++)
                {
                    this.m_Displays[i].OnDraw(ref rect, this.m_GUIStyle);
                }
            }

            private List<DebugDisplay> m_Displays;
            private GUIStyle m_GUIStyle;
        }

        public class ModsDisplay : DebugDisplay
        {
            public override void OnSetUp()
            {
            }

            public override void OnUpdate()
            {
            }

            public override void OnDraw(ref Rect rect, GUIStyle style)
            {
                if (shouldDisplay)
                {
                    base.DrawText(ref rect, style, this.m_Text);
                }
            }

            public string m_Text = string.Empty;
        }

        public interface Serialisable
        {
            void Serialise(BitStreamWriter writer);
            bool Deserialise(BitStreamReader reader);
        }

        [HarmonyPatch(typeof(GameModes.ClientCampaignMode), "OnOutro")]
        [HarmonyPostfix]
        private static void OnOutro()
        {
            if (isshow.Value)
            {

                if (PartyKevin.kevinEnabled.Value)
                {
                    bool shouldNtDisplayKevinState = true;
                    bool shouldNtDisplayNormalState = true;
                    modsDisplay.m_Text = $"自定义关卡:";
                    if (MServerLobbyFlowController.sceneDisableConfigEntries["只玩凯文和小节关"].Value)
                    {
                        modsDisplay.m_Text += "\n------以下为自定义特殊关卡:";
                        modsDisplay.m_Text += "\n只玩凯文和小节关";
                        shouldNtDisplayNormalState = false; //不输出普通状态
                    }
                    if (MServerLobbyFlowController.sceneDisableConfigEntries["不玩凯文和小节关"].Value)
                    {
                        modsDisplay.m_Text += "\n------以下为自定义特殊关卡:";
                        modsDisplay.m_Text += "\n不玩凯文和小节关";
                        shouldNtDisplayKevinState = false; //不输出凯文状态
                    }
                    if (MServerLobbyFlowController.sceneDisableConfigEntries["只玩麻3-4"].Value)
                    {
                        modsDisplay.m_Text += "\n------以下为自定义特殊关卡:";
                        modsDisplay.m_Text += "\n只玩麻3-4";
                        modsDisplay.m_Text += "\n请不要一直在街机内选麻3-4!" +
                            "\n此功能只是给豪车一个麻团的机会," +
                            "\n不是让你在街机练习!" +
                            "\n考虑一下别人的感受!" +
                            "\n想练习去steam群组叫车好吧?";
                        shouldNtDisplayKevinState = false; //只玩麻, 则不输出凯文状态
                        shouldNtDisplayNormalState = false; //也不输出普通状态
                    }
                    if (MServerLobbyFlowController.sceneDisableConfigEntries["只玩海3-4"].Value)
                    {
                        modsDisplay.m_Text += "\n------以下为自定义特殊关卡:";
                        modsDisplay.m_Text += "\n只玩海3-4";
                        modsDisplay.m_Text += "\n请不要一直在街机内选海3-4!" +
                            "\n此功能只是给豪车一个打海的机会," +
                            "\n不是让你在街机练习!" +
                            "\n考虑一下别人的感受!" +
                            "\n想练习去steam群组叫车好吧?";
                        shouldNtDisplayKevinState = false; //只玩海, 则不输出凯文状态
                        shouldNtDisplayNormalState = false; //也不输出普通状态
                    }
                    if (shouldNtDisplayKevinState)
                    {

                        List<string> conditions1 = new List<string>{
                                                                    "01-关闭小节关",
                                                                    "02-关闭主线凯文",
                                                                    "03-关闭海滩凯文",
                                                                    "04-关闭完美露营地凯文",
                                                                    "05-关闭恐怖地宫凯文",
                                                                    "06-关闭翻滚帐篷凯文",
                                                                    "07-关闭咸咸马戏团凯文",
                                                                    };
                        foreach (string condition in conditions1)
                        {
                            if (MServerLobbyFlowController.sceneDisableConfigEntries[condition].Value)
                            {
                                modsDisplay.m_Text += $"\n{condition}"; //凯文状态
                            }
                        }
                    }
                    if (shouldNtDisplayNormalState)
                    {
                        modsDisplay.m_Text += "\n------以下为自定义普通关卡:"; //凯文状态后

                        List<string> conditions = new List<string>{
                                                                "01-关闭世界1",
                                                                "02-关闭世界2",
                                                                "03-关闭世界3",
                                                                "04-关闭世界4",
                                                                "05-关闭世界5",
                                                                "06-关闭世界6",
                                                                "07-关闭节庆大餐",
                                                                "08-关闭王朝餐厅",
                                                                "09-关闭桃子游行",
                                                                "10-关闭幸运灯笼",
                                                                "11-关闭海滩",
                                                                "12-关闭烧烤度假村",
                                                                "13-关闭完美露营地",
                                                                "14-关闭美味树屋",
                                                                "15-关闭恐怖地宫",
                                                                "16-关闭惊悚庭院",
                                                                "17-关闭凶残城垛",
                                                                "18-关闭翻滚帐篷",
                                                                "19-关闭咸咸马戏团"
                                                                };
                        bool addedNormaltext = false;
                        foreach (string condition in conditions)
                        {
                            if (MServerLobbyFlowController.sceneDisableConfigEntries[condition].Value)
                            {
                                modsDisplay.m_Text += $"\n{condition}";
                                addedNormaltext = true; //启用了自定义普通关卡功能
                            }
                        }
                        if (!addedNormaltext)
                        {
                            modsDisplay.m_Text += $"\n没有自定义普通关卡"; //没启用时的消息
                        }
                    }

                }
                else
                {
                    modsDisplay.m_Text = "没有开启自定义关卡(凯文),请打开'02区域总开关'";
                }
                shouldDisplay = true;
            }
            else
            {
                shouldDisplay = false;
            }
        }
        private static Color HexToColor(string hex)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }
        [HarmonyPatch(typeof(LoadingScreenFlow), nameof(LoadingScreenFlow.LoadScene))]
        [HarmonyPrefix]
        private static void LoadScene()
        {
            shouldDisplay = false;
        }
    }
}