using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LobbyMODS
{
    internal class QuitWhenLoadScene
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> quitKey;

        public static void Awake()
        {
            quitKey = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "11-加载界面卡死退出", KeyCode.Backspace);
        }
        public static void Update()
        {
            //加载界面退出键
            if (Input.GetKeyDown(quitKey.Value))
            {
                LoadingScreenFlow LoadingScreenFlow = MODEntry.FindObjectOfType<LoadingScreenFlow>();
                if (LoadingScreenFlow != null)
                {
                    log("退出战局");
                    Traverse.Create(LoadingScreenFlow).Method("RequestReturnToStartScreen").GetValue();
                }
            }
        }
    }
}
