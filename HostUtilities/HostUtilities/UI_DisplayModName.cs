using BepInEx.Configuration;
using System.Collections.Generic;
using Team17.Online.Multiplayer.Connection;
using Team17.Online;
using UnityEngine;
using Team17.Online.Multiplayer;
using HarmonyLib;
using System;
using System.Reflection;
using System.Linq;

namespace HostUtilities
{
    public class UI_DisplayModName
    {
        private static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        private static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        private static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);

        public static string cornerMessage = $"Host Utilities v{MODEntry.Version} ";
        private static Harmony HarmonyInstance { get; set; }
        private static MyOnScreenDebugDisplay onScreenDebugDisplay;
        private static NetworkStateDebugDisplay NetworkDebugUI = null;
        private static bool canAdd;

        static List<Vector2> availablePositions;
        static Vector2 currentPosition;

        public static void Awake()
        {
            canAdd = false;
            availablePositions = new List<Vector2> {
                    new Vector2(0f, 0f), //左上
                    new Vector2(0f, Screen.height), // 左下
                    new Vector2(Screen.width, Screen.height), // 右下
                    new Vector2(Screen.width, 60 * MODEntry.dpiScaleFactor) //右上偏下60
                };
            onScreenDebugDisplay = new MyOnScreenDebugDisplay();
            onScreenDebugDisplay.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        public static void Update()
        {
            onScreenDebugDisplay.Update();

            if (canAdd)
            {
                AddNetworkDebugUI();
                canAdd = false;
            }
        }

        public static void OnDestroy()
        {
            RemoveNetworkDebugUI();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MetaGameProgress), "ByteLoad")]
        public static void MetaGameProgressByteLoadPatch(MetaGameProgress __instance)
        {
            canAdd = true;
        }

        public static void OnGUI() => onScreenDebugDisplay.OnGUI();

        public static void SetRandomPosition()
        {

            if (MODEntry.isInLobby)
            {
                // 如果在大厅中，则排除左上和右上
                availablePositions = new List<Vector2> {
                    new Vector2(0f, 20 * MODEntry.dpiScaleFactor), //左上偏下20
                    new Vector2(0f, Screen.height), // 左下
                    new Vector2(Screen.width, Screen.height), // 右下
                    new Vector2(Screen.width, 60 * MODEntry.dpiScaleFactor)  //右上偏下60
                };
            }
            else
            {
                // 否则使用所有预定义位置
                availablePositions = new List<Vector2> {
                    new Vector2(0f, 0f), //左上
                    new Vector2(0f, Screen.height), // 左下
                    new Vector2(Screen.width, Screen.height), // 右下
                    new Vector2(Screen.width, 60 * MODEntry.dpiScaleFactor) //右上偏下60
                };
            }
            System.Random rand = new System.Random();
            currentPosition = availablePositions[rand.Next(availablePositions.Count)];
        }

        private static void AddNetworkDebugUI()
        {
            NetworkDebugUI = new NetworkStateDebugDisplay();
            onScreenDebugDisplay.AddDisplay(NetworkDebugUI);
        }

        private static void RemoveNetworkDebugUI()
        {
            onScreenDebugDisplay.RemoveDisplay(NetworkDebugUI);
            NetworkDebugUI.OnDestroy();
            NetworkDebugUI = null;
        }

        private class MyOnScreenDebugDisplay
        {
            private readonly List<DebugDisplay> m_Displays = new List<DebugDisplay>();
            private static GUIStyle m_GUIStyle = new GUIStyle();
            public static string GetTextAlignment()
            {
                return m_GUIStyle.alignment.ToString();
            }

            public void AddDisplay(DebugDisplay display)
            {
                if (display != null)
                {
                    display.OnSetUp();
                    m_Displays.Add(display);
                }
            }

            public void RemoveDisplay(DebugDisplay display) => m_Displays.Remove(display);

            public void Awake()
            {
                m_GUIStyle.fontSize = Mathf.RoundToInt(20f * MODEntry.dpiScaleFactor);
                //m_GUIStyle.normal.textColor = new Color(145 / 255f, 195 / 255f, 228 / 255f, 0.3f);
                //m_GUIStyle.normal.textColor = new Color(1f, 1f, 1f, 1f);
                m_GUIStyle.normal.textColor = new Color(0f, 0f, 0f, 0.3f);
                m_GUIStyle.richText = false;
            }

            public void Update()
            {
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnUpdate();
            }

            public void OnGUI()
            {
                m_GUIStyle.fontSize = Mathf.RoundToInt(20f * MODEntry.dpiScaleFactor);
                Vector2 textSize = m_GUIStyle.CalcSize(new GUIContent(cornerMessage));
                float offset = 2f;
                int index = availablePositions.IndexOf(currentPosition);
                Rect rect = new Rect(currentPosition.x, currentPosition.y, textSize.x, textSize.y);
                switch (index)
                {
                    case 0:
                        m_GUIStyle.alignment = TextAnchor.UpperLeft;
                        rect.x += offset;
                        rect.y += offset;
                        break;
                    case 1:
                        m_GUIStyle.alignment = TextAnchor.LowerLeft;
                        rect.x += offset;
                        rect.y -= (textSize.y + offset);
                        break;
                    case 2:
                        m_GUIStyle.alignment = TextAnchor.LowerRight;
                        rect.x -= (textSize.x + offset);
                        rect.y -= (textSize.y + offset);
                        break;
                    case 3:
                        m_GUIStyle.alignment = TextAnchor.UpperRight;
                        rect.x -= (textSize.x + offset);
                        rect.y += offset;
                        break;
                }

                for (int i = 0; i < m_Displays.Count; i++)
                {
                    m_Displays[i].OnDraw(ref rect, m_GUIStyle);
                }
            }
        }

        public class NetworkStateDebugDisplay : DebugDisplay
        {
            public override void OnSetUp()
            {
            }

            public override void OnUpdate()
            {
            }

            public override void OnDraw(ref Rect rect, GUIStyle style)
            {
                DrawText(ref rect, style, cornerMessage);
            }
        }
    }
}
