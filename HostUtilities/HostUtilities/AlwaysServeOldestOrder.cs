using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using OrderController;
using BepInEx.Configuration;
using System.Reflection;
using System;

namespace HostUtilities
{
    public static class AlwaysServeOldestOrder
    {
        private static ConfigEntry<bool> AlwaysServeOldestOrderenabled;
        public static Harmony HarmonyInstance { get; set; }
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static void Awake()
        {
            AlwaysServeOldestOrderenabled = _MODEntry.Instance.Config.Bind<bool>("01-功能开关", "02-总是优先服务最左侧的订单(仅街机)", true);
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            _MODEntry.AllHarmony.Add(HarmonyInstance);
            _MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        /* Ripped From IL (Private Method) */
        private static bool Matches(OrderDefinitionNode _required, AssembledDefinitionNode _provided, PlatingStepData _plateType)
        {
            if (_required.m_platingStep != _plateType)
            {
                return false;
            }
            if (_required.GetType() == typeof(WildcardOrderNode))
            {
                return AssembledDefinitionNode.Matching(_required, _provided);
            }
            return AssembledDefinitionNode.Matching(_provided, _required);
        }

        /* Completely replaces the original */
        [HarmonyPatch(typeof(ServerOrderControllerBase), nameof(ServerOrderControllerBase.FindBestOrderForRecipe))]
        [HarmonyPostfix]
        private static void FindBestOrderForRecipe(ref AssembledDefinitionNode _order, ref PlatingStepData _plateType, ref OrderID o_orderID, ref float _timePropRemainingPercentage, ref bool __result, ref List<ServerOrderData> ___m_activeOrders)
        {
            try
            {
                // bool cheatServe = OC2Config.Config.CheatsEnabled;
                bool cheatServe = false;

                if (!AlwaysServeOldestOrderenabled.Value && !cheatServe && !_MODEntry.IsInParty)
                {
                    log("AlwaysServeOldestOrder is disabled");
                    return;
                }

                if (cheatServe)
                {
                    __result = true;
                }

                if (!__result)
                {
                    return; // we won't do any better if no orders matched
                }

                o_orderID = new OrderID(0U);
                _timePropRemainingPercentage = 0f;
                for (int i = ___m_activeOrders.Count - 1; i >= 0; i--)
                {
                    ServerOrderData order = ___m_activeOrders[i];
                    if (Matches(order.RecipeListEntry.m_order, _order, _plateType) || cheatServe)
                    {
                        o_orderID = order.ID;
                        _timePropRemainingPercentage = Mathf.Clamp01(order.Remaining / order.Lifetime);
                    }
                }

            }
            catch (Exception e)
            {
                _MODEntry.LogError($"An error occurred: \n{e.Message}");
                _MODEntry.LogError($"Stack trace: \n{e.StackTrace}");
            }
        }
    }
}