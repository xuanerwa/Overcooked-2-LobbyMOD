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
    public class HeatedPositionFix
    {
        public static ConfigEntry<bool> heatedPositionFixEnabled;
        public static Harmony HarmonyInstance { get; set; }
        public static void Awake()
        {
            heatedPositionFixEnabled = _MODEntry.Instance.Config.Bind("00-UI", "06-海滩火力进度条偏移", true, "海滩火力进度条向上挪动, 不与食物进度条重叠");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClientHeatedStationGUI), "StartSynchronising")]
        public static bool ClientHeatedStationGUI_StartSynchronising_Prefix(ref ClientHeatedStationGUI __instance, Component synchronisedObject)
        {
            if (heatedPositionFixEnabled.Value)
            {
                _MODEntry.LogInfo("HeatedPositionFix");
                __instance.m_heatedStationGUI = (HeatedStationGUI)synchronisedObject;
                __instance.m_heatedStation = __instance.gameObject.RequireComponent<ClientHeatedStation>();
                __instance.m_heatedStationGUI.m_Offset = __instance.m_heatedStationGUI.m_Offset.AddY(2f);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
