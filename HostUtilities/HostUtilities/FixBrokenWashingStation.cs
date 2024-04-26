using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class FixBrokenWashingStation
    {
        public static Harmony HarmonyInstance { get; set; }
        public static string currentString;
        public static ConfigEntry<bool> showAttachSituation;
        //public static ConfigEntry<bool> isFixBrokenWashingStation;
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static void Awake()
        {
            showAttachSituation = _MODEntry.Instance.Config.Bind("00-UI", "07-Console中显示Attatch详情", false, "如您不知道此选项是干嘛的, 请不要开启, 若开启, 当两个以上的物体卡在同一个格子上, 会导致大量掉帧");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ServerAttachStation), "CanAttachToSelf")]
        public static bool ServerAttachStation_CanAttachToSelf_Prefix(ref ServerAttachStation __instance, UnityEngine.GameObject _item, PlacementContext _context)
        {
            if (__instance.name == "WashingPart" || __instance.name == "Switch(1)" || __instance.name == "workstation_cooker_01")
            {
                return false;
            }
            else
            {
                if (showAttachSituation.Value)
                {
                    log($"CanAttachToSelf: InstanceName: 【{__instance.name}】, ObjectName: 【{_item.name}】");
                }
                return true;
            }
        }
    }
}
