using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HostUtilities
{
    public class FixHeatedPosition
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<bool> heatedPositionFixEnabled;
        public static Harmony HarmonyInstance { get; set; }
        public static void Awake()
        {
            heatedPositionFixEnabled = MODEntry.Instance.Config.Bind("00-UI", "06-海滩火力进度条偏移", true, "海滩火力进度条向上挪动, 不与食物进度条重叠");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClientHeatedStationGUI), "StartSynchronising")]
        public static bool ClientHeatedStationGUI_StartSynchronising_Prefix(ref ClientHeatedStationGUI __instance, Component synchronisedObject)
        {
            try
            {
                if (heatedPositionFixEnabled.Value)
                {
                    Log("HeatedPositionFix");
                    __instance.m_heatedStationGUI = (HeatedStationGUI)synchronisedObject;
                    __instance.m_heatedStationGUI.m_Offset = Vector3.up;
                    __instance.m_heatedStationGUI.m_Offset = __instance.m_heatedStationGUI.m_Offset.AddY(2f);
                    __instance.m_heatedStation = __instance.gameObject.RequireComponent<ClientHeatedStation>();
                    __instance.m_heatValue = __instance.m_heatedStation.HeatValue;
                    __instance.OnHeatValueChanged(__instance.m_heatValue);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                LogE($"An error occurred: \n{e.Message}");
                LogE($"Stack trace: \n{e.StackTrace}");
                return true;
            }
        }
    }
}
