using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    internal class QuitInLoadingScreen
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<KeyCode> quitKey;

        public static void Awake()
        {
            quitKey = MODEntry.Instance.Config.Bind<KeyCode>("02-按键绑定", "11-加载界面卡死退出", KeyCode.Backspace);
        }
        public static void Update()
        {
            //加载界面退出键
            if (Input.GetKeyDown(quitKey.Value))
            {
                LoadingScreenFlow LoadingScreenFlow = MODEntry.FindObjectOfType<LoadingScreenFlow>();
                if (LoadingScreenFlow != null)
                {
                    Log("退出战局");
                    LoadingScreenFlow.RequestReturnToStartScreen();
                }
            }
        }
    }
}
