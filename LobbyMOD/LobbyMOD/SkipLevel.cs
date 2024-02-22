using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using Team17.Online;
using GameModes.Horde;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;

namespace LobbyMODS
{
    public class SkipLevel
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> stopLevel;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            stopLevel = MODEntry.Instance.Config.Bind("01-按键绑定", "10-直接跳过关卡", KeyCode.Delete, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(stopLevel.Value))
            {

                if (!cooling)
                {
                    log("跳过关卡");
                    EndLevel();
                }
                if (System.Environment.TickCount - startTime > 10000)
                {
                    cooling = false;
                    log("跳过关卡");
                    EndLevel();
                }
            }
        }


        public static void EndLevel()
        {
            if (!IsHostPlayer())
            {
                log("不是主机玩家");
                return;
            }

            ServerCampaignFlowController flowController = GameObject.FindObjectOfType<ServerCampaignFlowController>();
            ServerHordeFlowController ServerHordeFlowController = GameObject.FindObjectOfType<ServerHordeFlowController>();
            if (flowController == null)
            {
                log("flowController为空");
                if (ServerHordeFlowController == null)
                {
                    log("ServerHordeFlowController为空");
                    return;
                }
                else
                {
                    ServerHordeFlowController.Damage(100);
                    log("跳过敌群关卡");
                    startTime = System.Environment.TickCount;
                    cooling = true;
                }
            }
            else
            {
                flowController.SkipToEnd();
                log("跳过普通关卡");
                startTime = System.Environment.TickCount;
                cooling = true;
            }

        }

        public static bool IsHostPlayer()
        {
            IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
            if (onlinePlatformManager == null)
            {
                return true;
            }

            IOnlineMultiplayerSessionCoordinator coordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
            if (coordinator == null)
            {
                return true;
            }

            if (coordinator.IsIdle())
            {
                return true;
            }

            return coordinator.IsHost();
        }
    }
}
