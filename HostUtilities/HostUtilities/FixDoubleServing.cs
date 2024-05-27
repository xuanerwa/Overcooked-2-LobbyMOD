using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
namespace HostUtilities
{
    public class FixDoubleServing
    {
        public static Harmony HarmonyInstance { get; set; }
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<bool> isDoubleServingBanned;
        public static ConfigEntry<bool> isOneRecipeDoubleServingAllowed;
        public static bool isOnlyOneRecipe = false;
        public static bool isSushi1_1 = false;
        public static void Awake()
        {
            isDoubleServingBanned = _MODEntry.Instance.Config.Bind<bool>("01-功能开关", "01-00-禁止卡盘", true, "禁止卡盘子");
            isOneRecipeDoubleServingAllowed = _MODEntry.Instance.Config.Bind<bool>("01-功能开关", "01-01-只有一个食谱时允许卡盘", false, "只有一个食谱时允许卡盘子");
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        private static bool skipNext = false;

        [HarmonyPatch(typeof(ServerPlateStation), "DeliverCurrentPlate")]
        [HarmonyPrefix]
        private static void DeliverCurrentPlate(ref ServerPlateStation __instance, ref ServerPlate ___m_plate, ref IKitchenOrderHandler ___m_orderHandler)
        {
            try
            {
                if (isDoubleServingBanned.Value && ___m_plate.IsReserved())
                {
                    skipNext = true;
                }
            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
            }
        }

        [HarmonyPatch(typeof(ServerKitchenFlowControllerBase), nameof(ServerKitchenFlowControllerBase.FoodDelivered))]
        [HarmonyPrefix]
        private static bool FoodDelivered(ref AssembledDefinitionNode _definition, ref PlatingStepData _plateType, ref ServerPlateStation _station)
        {
            try
            {
                if (skipNext)
                {
                    skipNext = false;
                    if (isOneRecipeDoubleServingAllowed.Value && isOnlyOneRecipe)
                    {
                        log($"拦截到卡盘子, 但 单一菜单关 或 主线1-1 允许卡盘");
                        return true;
                    }
                    log($"拦截到卡盘子!");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
                return true;
            }
        }

        [HarmonyPatch(typeof(ScriptedRoundData), "InitialiseRound")]
        [HarmonyPostfix]
        private static void ScriptedRoundData_InitialiseRound_Postfix(ScriptedRoundData __instance)
        {
            try
            {
                isSushi1_1 = false;
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "FlowManager")
                    {
                        ServerCampaignFlowController controller = obj.GetComponent<ServerCampaignFlowController>();
                        if (controller != null)
                        {
                            LevelConfigBase levelConfig = controller.GetLevelConfig();
                            if (levelConfig != null)
                            {
                                log($"本关关卡名称: {levelConfig.name}");
                                if (levelConfig.name.Contains("Sushi_1_1"))
                                {
                                    isSushi1_1 = true;
                                }
                            }
                        }
                    }
                }
                if (__instance.m_recipes.m_recipes.Length > 1)
                {
                    log($"本关菜单种类有 {__instance.m_recipes.m_recipes.Length} 种");
                    isOnlyOneRecipe = false;
                }
                else
                {
                    log("本关菜单种类有 1 种");
                    isOnlyOneRecipe = true;
                }
                for (int i = 0; i < __instance.m_recipes.m_recipes.Length; i++)
                {
                    var item = __instance.m_recipes.m_recipes[i];
                    log($"本关菜单索引: {i}, 菜单名: {item.m_order.name}");
                }
                if (isSushi1_1)
                {
                    isOnlyOneRecipe = true;
                }
            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
            }
        }

    }
}
