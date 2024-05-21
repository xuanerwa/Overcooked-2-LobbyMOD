using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace HostUtilities
{
    internal class QuitInLoadingScreen
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> quitKey;

        public static void Awake()
        {
            quitKey = _MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "11-加载界面卡死退出", KeyCode.Backspace);
        }
        public static void Update()
        {
            //加载界面退出键
            if (Input.GetKeyDown(quitKey.Value))
            {
                LoadingScreenFlow LoadingScreenFlow = _MODEntry.FindObjectOfType<LoadingScreenFlow>();
                if (LoadingScreenFlow != null)
                {
                    log("退出战局");
                    LoadingScreenFlow.RequestReturnToStartScreen();
                }
            }
        }
    }
}
