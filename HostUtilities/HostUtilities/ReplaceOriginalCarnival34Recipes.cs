using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static ClientPortalMapNode;
namespace HostUtilities
{
    public class ReplaceOriginalCarnival34Recipes
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<int> timeout;
        public static ConfigEntry<string> functionEnabled;
        public static Harmony HarmonyInstance { get; set; }
        private static readonly string[] strList = {
            "游戏原始菜单逻辑",
            "DLL里的菜单逻辑",
        };
        public static void Awake()
        {
            functionEnabled = MODEntry.Instance.Config.Bind("05-菜单逻辑", "01-选择菜单逻辑", strList[0], new ConfigDescription("原始菜单逻辑为 不受你自己替换的dll影响, 原汁原味的菜单逻辑. DLL里的菜单逻辑就是 使用dll的菜单逻辑, 如果你替换过好菜单dll, 则此选项即使用好菜单.", new AcceptableValueList<string>(strList)));
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        //[HarmonyPatch(typeof(RoundData), "GetWeight")]
        //[HarmonyPostfix]
        //private static bool RoundData_GetWeight_Prefix(RoundData __instance, ref float __result)
        //{
        //    try
        //    {
        //        bool flag = functionEnabled.Value.Equals("游戏原始菜单逻辑");
        //        if (flag)
        //        {
        //            Log("原始菜单逻辑");
        //            int num = _instance.CumulativeFrequencies.Collapse((int f, int total) => total + f);
        //            float num2 = (num + 2) / __instance.m_recipes.m_recipes.Length;
        //            __result = Mathf.Max(num2 - _instance.CumulativeFrequencies[_recipeIndex], 0f);
        //            return false;
        //        }
        //        else
        //        {
        //            Log("DLL菜单逻辑");
        //            return true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogE($"An error occurred: \n{e.Message}");
        //        LogE($"Stack trace: \n{e.StackTrace}");
        //        return true;
        //    }
        //}


    }
}

