using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class ModifyMaxActiveOrders
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<int> n_maxActiveOrders;
        public static ConfigEntry<float> n_orderLifetime;
        public static ConfigEntry<float> n_timeBetweenOrders;
        public static ConfigEntry<bool> functionEnabled;
        public static Harmony HarmonyInstance { get; set; }

        public static void Awake()
        {
            functionEnabled = _MODEntry.Instance.Config.Bind("00-菜单相关", "修改菜单总开关", false, "关闭后下一局生效");
            n_maxActiveOrders = _MODEntry.Instance.Config.Bind("00-菜单相关", "00-最大单量", 5, new ConfigDescription("订单数量, 只在街机才有效, 且只对Campaign模式有用, 无尽模式和生存模式还有带场景切换的地图(例如篝火3-4,海滩3-4等)无效", new AcceptableValueRange<int>(3, 8)));
            n_orderLifetime = _MODEntry.Instance.Config.Bind("00-菜单相关", "01-超时时间", 210f, new ConfigDescription("超时时间, 只在街机才有效, 且只对Campaign模式有用, 无尽模式和生存模式还有带场景切换的地图(例如篝火3-4,海滩3-4等)无效", new AcceptableValueRange<float>(110f, 310f))); ;
            n_timeBetweenOrders = _MODEntry.Instance.Config.Bind("00-菜单相关", "02-订单出现间隔时间", 15f, new ConfigDescription("两单间隔时间, 只在街机才有效, 且只对Campaign模式有用, 无尽模式和生存模式还有带场景切换的地图(例如篝火3-4,海滩3-4等)无效", new AcceptableValueRange<float>(5f, 30f))); ;
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ServerCampaignMode), "BuildOrderConfig")]
        public static void ServerCampaignMode_BuildOrderConfig(ref ServerFixedTimeOrderController.OrdersConfig __result)
        {
            if (!functionEnabled.Value) { return; }
            if (!_MODEntry.IsInParty) { return; }

            __result.m_maxActiveOrders = n_maxActiveOrders.Value;
            __result.m_orderLifetime = n_orderLifetime.Value;
            __result.m_timeBetweenOrders = n_timeBetweenOrders.Value;
            log($"__result.m_maxActiveOrders {__result.m_maxActiveOrders}");
            log($"__result.m_roundData {__result.m_roundData}");
            log($"__result.m_orderLifetime {__result.m_orderLifetime}");
            log($"__result.m_timeBetweenOrders {__result.m_timeBetweenOrders}");
        }
    }
}
