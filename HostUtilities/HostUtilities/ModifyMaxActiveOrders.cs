using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using System.Reflection;
namespace HostUtilities
{
    public class ModifyMaxActiveOrders
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<int> n_maxActiveOrders;
        public static ConfigEntry<float> n_orderLifetime;
        public static ConfigEntry<float> n_timeBetweenOrders;
        public static ConfigEntry<bool> functionEnabled;
        public static Harmony HarmonyInstance { get; set; }

        public static void Awake()
        {
            functionEnabled = MODEntry.Instance.Config.Bind("05-菜单相关", "00-修改菜单总开关", false, "关闭后下一局生效");
            n_maxActiveOrders = MODEntry.Instance.Config.Bind("05-菜单相关", "01-最大单量", 0, new ConfigDescription("订单数量", new AcceptableValueRange<int>(-2, 2)));
            n_orderLifetime = MODEntry.Instance.Config.Bind("05-菜单相关", "02-超时时间", 0f, new ConfigDescription("超时时间", new AcceptableValueRange<float>(-150f, 150f)));
            n_timeBetweenOrders = MODEntry.Instance.Config.Bind("05-菜单相关", "03-订单出现间隔时间", 0f, new ConfigDescription("两单间隔时间", new AcceptableValueRange<float>(-30f, 30f))); ;
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ServerCampaignMode), "BuildOrderConfig")]
        public static void ServerCampaignMode_BuildOrderConfig(ref ServerFixedTimeOrderController.OrdersConfig __result)
        {
            try
            {
                if (!functionEnabled.Value) { return; }
                if (!MODEntry.isInParty) { return; }
                Log("------------------------------------");
                Log($"原始最大单量 {__result.m_maxActiveOrders}");
                Log($"原始订单超时 {__result.m_orderLifetime}");
                Log($"原始订单间隔 {__result.m_timeBetweenOrders}");
                Log("------------------------------------");
                __result.m_maxActiveOrders += n_maxActiveOrders.Value;
                __result.m_orderLifetime += n_orderLifetime.Value;
                __result.m_timeBetweenOrders += n_timeBetweenOrders.Value;

                if (__result.m_timeBetweenOrders < 0) { __result.m_timeBetweenOrders = 0; }
                Log($"修改最大单量 {__result.m_maxActiveOrders}");
                Log($"修改订单超时 {__result.m_orderLifetime}");
                Log($"修改订单间隔 {__result.m_timeBetweenOrders}");
                Log("------------------------------------");
            }
            catch (System.Exception e)
            {
                LogE($"An error occurred: \n{e.Message}");
                LogE($"Stack trace: \n{e.StackTrace}");
            }
        }
    }
}
