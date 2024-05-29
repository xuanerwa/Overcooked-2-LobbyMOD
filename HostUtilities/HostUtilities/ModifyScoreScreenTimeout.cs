using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using System.Reflection;
namespace HostUtilities
{
    public class ModifyScoreScreenTimeout
    {
        public static void Log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<int> timeout;
        public static ConfigEntry<bool> functionEnabled;
        public static Harmony HarmonyInstance { get; set; }

        public static void Awake()
        {
            functionEnabled = _MODEntry.Instance.Config.Bind("01-功能开关", "06-00-启用修改结算页超时", false, "关闭后下一局生效");
            timeout = _MODEntry.Instance.Config.Bind("01-功能开关", "06-01-修改结算页超时为", 10, new ConfigDescription("超时", new AcceptableValueRange<int>(5, 20)));
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScoreScreenOutroFlowroutine), "Setup")]
        public static void ScoreScreenOutroFlowroutine_Setup_Postfix(ref ScoreScreenOutroFlowroutine __instance)
        {
            try
            {
                if (!functionEnabled.Value) { return; }
                if (!_MODEntry.IsHost) { return; }

                Log("------------------------------------");
                Log($"原始结算页面超时时间 {__instance.m_flowroutineData.m_fTimeout}");
                Log("------------------------------------");
                __instance.m_flowroutineData.m_fTimeout = timeout.Value;
                Log($"修改结算页面超时时间 {__instance.m_flowroutineData.m_fTimeout}");
                Log("------------------------------------");
            }
            catch (System.Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
            }
        }
    }
}
