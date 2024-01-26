﻿using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace LobbyMODS
{
    internal class ForceHost
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<string> ValueList;
        private static string[] strList = {
            "游戏默认逻辑",
            "强制主机",
            "强制客机"
        };
        public static void Awake()
        {
            ValueList = MODEntry.Instance.Config.Bind<string>("00-功能开关", "切换默认主机/客机角色:", strList[0], new ConfigDescription("选择状态", new AcceptableValueList<string>(strList)));

            Harmony.CreateAndPatchAll(typeof(ForceHost));
        }
        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                ClientLobbyFlowController lobbyFlowController = MODEntry.FindObjectOfType<ClientLobbyFlowController>();

                if (lobbyFlowController != null)
                {
                    Traverse.Create(lobbyFlowController).Method("TryJoinGame").GetValue();
                    log("尝试以客机身份加入游戏");
                }
                else
                {
                    log("未找到 ClientLobbyFlowController 实例");
                }
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                ClientLobbyFlowController lobbyFlowController = MODEntry.FindObjectOfType<ClientLobbyFlowController>();

                if (lobbyFlowController != null)
                {
                    Traverse.Create(lobbyFlowController).Method("HostGame").GetValue();
                    log("强制以主机身份主持游戏");
                }
                else
                {
                    log("未找到 ClientLobbyFlowController 实例");
                }
            }

        }

        [HarmonyPatch(typeof(ClientLobbyFlowController), "HostGame")]
        [HarmonyPostfix]
        private static void ClientLobbyFlowController_HostGame_Prefix(ClientLobbyFlowController __instance)
        {
            bool flag = ForceHost.ValueList.Value.Equals("强制客机");
            if (flag)
            {
                MODEntry.LogWarning("强制客机已生效, TryJoinGame");
                Traverse.Create(__instance).Method("TryJoinGame", new object[0]).GetValue();
            }
        }
        [HarmonyPatch(typeof(ClientLobbyFlowController), "TryJoinGame")]
        [HarmonyPrefix]
        private static bool ClientLobbyFlowController_TryJoinGame_Prefix(ClientLobbyFlowController __instance)
        {
            bool flag = ForceHost.ValueList.Value.Equals("强制主机");
            bool result;
            if (flag)
            {
                MODEntry.LogWarning("强制主机已生效");
                Traverse.Create(__instance).Method("HostGame", new object[0]).GetValue();
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
