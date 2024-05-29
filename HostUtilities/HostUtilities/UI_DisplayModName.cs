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
        public static string cornerMessage = $"Host Utilities v{_MODEntry.Version} ";
        public static Harmony HarmonyInstance { get; set; }
        private static MyOnScreenDebugDisplay onScreenDebugDisplay;
        private static NetworkStateDebugDisplay NetworkDebugUI = null;
        public static ConfigEntry<bool> ShowEnabled;
        public static ConfigEntry<bool> isShowDebugInfo;
        public static bool canAdd;

        private static readonly List<Vector2> predefinedPositions = new List<Vector2>
        {
            new Vector2(0f, 0f), // 左下
            new Vector2(Screen.width, 0f), // 右下
            new Vector2(0f, Screen.height), // 左上
            new Vector2(Screen.width, Screen.height), // 右上
        };

        private static Vector2 currentPosition = predefinedPositions[0];

        public static void Awake()
        {
            canAdd = false;
            onScreenDebugDisplay = new MyOnScreenDebugDisplay();
            onScreenDebugDisplay.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
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
            System.Random rand = new System.Random();
            currentPosition = predefinedPositions[rand.Next(predefinedPositions.Count)];
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
            private readonly GUIStyle m_GUIStyle = new GUIStyle();

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
                m_GUIStyle.fontSize = Mathf.RoundToInt(25f * _MODEntry.dpiScaleFactor);
                m_GUIStyle.normal.textColor = new Color(145 / 255f, 195 / 255f, 228 / 255f, 0.5f);
                m_GUIStyle.richText = false;
            }

            public void Update()
            {
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnUpdate();
            }

            public void OnGUI()
            {
                m_GUIStyle.fontSize = Mathf.RoundToInt(25f * _MODEntry.dpiScaleFactor);

                // 设置对齐方式
                if (currentPosition == predefinedPositions[0])
                {
                    m_GUIStyle.alignment = TextAnchor.LowerLeft;
                }
                else if (currentPosition == predefinedPositions[1])
                {
                    m_GUIStyle.alignment = TextAnchor.LowerRight;
                }
                else if (currentPosition == predefinedPositions[2])
                {
                    m_GUIStyle.alignment = TextAnchor.UpperLeft;
                }
                else if (currentPosition == predefinedPositions[3])
                {
                    m_GUIStyle.alignment = TextAnchor.UpperRight;
                }

                Rect rect = new Rect(currentPosition.x, Screen.height - m_GUIStyle.fontSize - currentPosition.y, 0f, m_GUIStyle.fontSize);
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnDraw(ref rect, m_GUIStyle);
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
