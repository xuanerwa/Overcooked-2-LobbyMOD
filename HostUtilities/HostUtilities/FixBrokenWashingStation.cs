using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class FixBrokenWashingStation
    {
        public static Harmony HarmonyInstance { get; set; }
        //public static ConfigEntry<bool> isFixBrokenWashingStation;
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static void Awake()
        {
            //isFixBrokenWashingStation = _MODEntry.Instance.Config.Bind("01-功能开关", "修复被砸坏的洗碗池", true);
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ServerAttachStation), "CanAttachToSelf")]
        public static bool ServerAttachStation_CanAttachToSelf_Prefix(ref ServerAttachStation __instance, UnityEngine.GameObject _item, PlacementContext _context)
        {
            if (__instance.name == "WashingPart")
            {
                return false;
            }
            else
            {
                log($"CanAttachToSelf: InstanceName: 【{__instance.name}】, ObjectName: 【{_item.name}】");
                return true;
            }
        }
    }
}
