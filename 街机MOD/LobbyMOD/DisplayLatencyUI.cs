using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace LobbyMODS
{
    public static class DisplayLatencyUI

    {
        private static MyOnScreenDebugDisplay onScreenDebugDisplay;
        private static MyTextUI TextUI = null;
        public static ConfigEntry<bool> ShowEnabled;


        public static void add_m_Text(string str) => TextUI?.add_m_Text(str);
        public static void change_m_Text(string str) => TextUI?.change_m_Text(str);

        public static void Awake()
        {
            onScreenDebugDisplay = new MyOnScreenDebugDisplay();
            onScreenDebugDisplay.Awake();
            ShowEnabled = MODEntry.Instance.Config.Bind<bool>("00-功能开关", "显示延迟", false);
        }

        public static void Update()
        {
            onScreenDebugDisplay.Update();

            if (TextUI != null && !ShowEnabled.Value)
            {

                RemoveTextUI();
            }
            else if (TextUI == null && ShowEnabled.Value)
            {
                AddTextUI();
            }
        }

        public static void OnGUI() => onScreenDebugDisplay.OnGUI();

        private static void AddTextUI()
        {
            TextUI = new MyTextUI();
            onScreenDebugDisplay.AddDisplay(TextUI);
            //TextUI.init_m_Text();
        }

        private static void RemoveTextUI()
        {
            onScreenDebugDisplay.RemoveDisplay(TextUI);
            TextUI.OnDestroy();
            TextUI = null;
        }

        public class MyTextUI : DebugDisplay
        {
            private bool replaced = false;

            //public void init_m_Text() => m_Text = string.Empty;

            public void add_m_Text(string str)
            {
                if (replaced) m_Text += "\n" + str;
                else { m_Text = str; replaced = true; }
            }
            public void change_m_Text(string str)
            {
                m_Text = str;
            }

            public override void OnSetUp() { }

            public override void OnUpdate() { }

            public override void OnDraw(ref Rect rect, GUIStyle style) => base.DrawText(ref rect, style, m_Text);

            private static string m_Text = string.Empty;
            //private static string m_Text = "这里显示延迟";
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
                m_GUIStyle.alignment = TextAnchor.UpperLeft;
                m_GUIStyle.fontSize = (int)(Screen.height * 0.015f);
                m_GUIStyle.normal.textColor = new Color(222 / 255.0f, 0f, 222 / 255.0f, 0.8f);
            }

            public void Update()
            {
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnUpdate();
            }

            public void OnGUI()
            {
                Rect rect = new Rect(0f, Screen.height * 0.035f, Screen.width, m_GUIStyle.fontSize);
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnDraw(ref rect, m_GUIStyle);
            }
        }
    }
}
