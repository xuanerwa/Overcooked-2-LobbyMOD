using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class FixBrokenWashingStation
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static Harmony HarmonyInstance { get; set; }
        public static string currentString;
        public static ConfigEntry<bool> showAttachSituation;
        public static void Awake()
        {
            showAttachSituation = MODEntry.Instance.Config.Bind("00-UI", "07-Console中显示Attatch详情", false, "如您不知道此选项是干嘛的, 请不要开启, 若开启, 当两个以上的物体卡在同一个格子上, 会导致大量掉帧");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ServerAttachStation), "CanAttachToSelf")]
        public static bool ServerAttachStation_CanAttachToSelf_Prefix(ref ServerAttachStation __instance, UnityEngine.GameObject _item)
        {
            try
            {
                if (__instance.name == "WashingPart" && MODEntry.isInParty)
                {
                    return false;
                }
                else
                {
                    if (showAttachSituation.Value)
                    {
                        Log($"CanAttachToSelf: InstanceName: 【{__instance.name}】, ObjectName: 【{_item.name}】");
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                LogE($"An error occurred: \n{e.Message}");
                LogE($"Stack trace: \n{e.StackTrace}");
                return false;
            }
        }
    }
}
