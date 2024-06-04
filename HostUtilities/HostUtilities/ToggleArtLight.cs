using BepInEx.Configuration;
using HarmonyLib;
using HostUtilities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ToggleArtLight : MonoBehaviour
{
    public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
    public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
    public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);

    public static Harmony HarmonyInstance { get; set; }
    private static ConfigEntry<KeyCode> toggleKey;
    private static GameObject artObjects = null;
    public static void Awake()
    {
        toggleKey = MODEntry.Instance.Config.Bind("02-按键绑定", "16-开关灯", KeyCode.L);
        HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
        MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
    }

    public static void Update()
    {
        if (Input.GetKeyDown(toggleKey.Value))
        {
            Log("Toggle Lights");
            ToggleArtLights();
        }
    }

    static void ToggleArtLights()
    {
        if (artObjects == null)
        {
            artObjects = GameObject.Find("Lights");
            if (artObjects == null)
                artObjects = GameObject.Find("TEMP_LIGHT");
        }
        if (artObjects != null)
        {
            bool isActive = artObjects.activeSelf;
            artObjects.SetActive(!isActive);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LoadingScreenFlow), "Awake")]
    private static void LoadingScreenFlow_Awake()
    {
        LogW("重置LightObject");
        artObjects = null;
    }
}
