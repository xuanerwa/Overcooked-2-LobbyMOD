using UnityEngine;
using BepInEx.Configuration;
using GameModes.Horde;
using System.Reflection;


namespace HostUtilities
{
    public class SkipLevel
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<KeyCode> stopLevel;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            stopLevel = MODEntry.Instance.Config.Bind("02-按键绑定", "10-直接跳过关卡", KeyCode.Delete, "跳过关卡");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(stopLevel.Value))
            {
                if (!MODEntry.isHost)
                {
                    MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                if (!cooling)
                {
                    Log("跳过关卡");
                    EndLevel();
                }
                if (System.Environment.TickCount - startTime > 5000)
                {
                    cooling = false;
                    Log("跳过关卡");
                    EndLevel();
                }
            }
        }


        public static void EndLevel()
        {
            ServerCampaignFlowController flowController = GameObject.FindObjectOfType<ServerCampaignFlowController>();
            ServerHordeFlowController ServerHordeFlowController = GameObject.FindObjectOfType<ServerHordeFlowController>();
            if (flowController == null)
            {
                Log("flowController为空");
                if (ServerHordeFlowController == null)
                {
                    Log("ServerHordeFlowController为空");
                    return;
                }
                else
                {
                    ServerHordeFlowController.Damage(100);
                    Log("跳过敌群关卡");
                    startTime = System.Environment.TickCount;
                    cooling = true;
                }
            }
            else
            {
                flowController.SkipToEnd();
                Log("跳过普通关卡");
                startTime = System.Environment.TickCount;
                cooling = true;
            }

        }
    }
}
