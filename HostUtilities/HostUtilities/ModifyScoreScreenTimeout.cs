using BepInEx.Configuration;
using GameModes;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
namespace HostUtilities
{
    public class ModifyScoreScreenTimeout
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<int> timeout;
        public static ConfigEntry<bool> functionEnabled;
        public static Harmony HarmonyInstance { get; set; }

        public static void Awake()
        {
            functionEnabled = MODEntry.Instance.Config.Bind("01-功能开关", "06-00-启用修改结算页超时", false, "关闭后下一局生效");
            timeout = MODEntry.Instance.Config.Bind("01-功能开关", "06-01-修改结算页超时为", 10, new ConfigDescription("超时", new AcceptableValueRange<int>(5, 2000)));
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }

        //[HarmonyTranspiler]
        //[HarmonyPatch(typeof(ScoreScreenOutroFlowroutine), "Run", MethodType.Enumerator)]
        //public static bool ScoreScreenOutroFlowroutine_Setup_Postfix(ScoreScreenOutroFlowroutine __instance)
        //{
        //    //try
        //    //{
        //    if (!functionEnabled.Value) { return true; }
        //    if (!_MODEntry.isHost) { return true; }

        //    Log("------------------------------------");
        //    Log($"修改结算页面超时时间 {__instance.m_flowroutineData.m_fTimeout}");
        //    Log("------------------------------------");


        //    int layer = LayerMask.NameToLayer("Administration");
        //    IEnumerator timeoutRoutine = CoroutineUtils.TimerRoutine(timeout.Value, layer);
        //    while (timeoutRoutine.MoveNext())
        //    {
        //        if (__instance.m_selectButton.JustPressed() || __instance.m_restartButton.JustPressed())
        //        {
        //            break;
        //        }
        //        Log("计时");
        //        return false;
        //    }
        //    return false;

        //    //}
        //    //catch (System.Exception e)
        //    //{
        //    //    LogE($"An error occurred: \n{e.Message}");
        //    //    LogE($"Stack trace: \n{e.StackTrace}");
        //    //}
        //}


        //[HarmonyPatch(typeof(ScoreScreenOutroFlowroutine), "<Run>c__Iterator0::MoveNext")]
        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        //{
        //    var codes = new List<CodeInstruction>(instructions);
        //    Label endLabel = generator.DefineLabel(); // 用于跳转的标签

        //    // 找到方法结束的地方并打上标签
        //    for (int i = codes.Count - 1; i >= 0; i--)
        //    {
        //        if (codes[i].opcode == OpCodes.Ret)
        //        {
        //            codes[i].labels.Add(endLabel);
        //            break;
        //        }
        //    }

        //    for (int i = 0; i < codes.Count; i++)
        //    {
        //        // 查找 `yield return null;` 的 IL 指令序列
        //        if (codes[i].opcode == OpCodes.Ldnull &&
        //            codes[i + 1].opcode == OpCodes.Stfld &&
        //            ((FieldInfo)codes[i + 1].operand).Name == "$current")
        //        {
        //            // 修改 IL 指令，将 `yield return null;` 替换为跳转到方法结尾
        //            codes[i] = new CodeInstruction(OpCodes.Br, endLabel);
        //            codes[i + 1] = new CodeInstruction(OpCodes.Nop); // 用 Nop 占位，保持索引一致
        //        }
        //    }

        //    return codes.AsEnumerable();
        //}
    }
}
