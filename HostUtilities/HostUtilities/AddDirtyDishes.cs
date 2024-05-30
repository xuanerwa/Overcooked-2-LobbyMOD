using BepInEx.Configuration;
using System.Reflection;
using UnityEngine;

namespace HostUtilities
{
    public class AddDirtyDishes
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static ConfigEntry<KeyCode> AddDirtyDishesKey;
        public static int startTime;
        public static bool cooling = false;
        public static int plateOrGlassNum = 0;

        public static void Awake()
        {
            AddDirtyDishesKey = MODEntry.Instance.Config.Bind("02-按键绑定", "13-增加1个脏盘/杯(仅街机)", KeyCode.Alpha0);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(AddDirtyDishesKey.Value))
            {
                if (!MODEntry.isHost)
                {
                    MODEntry.ShowWarningDialog("你不是主机，别点啦");
                    return;
                }
                if (!MODEntry.isInParty)
                {
                    MODEntry.ShowWarningDialog("请在街机中使用此功能。");
                    return;
                }

                GameObject DirtyPlateStackObject = GameObject.Find("DirtyPlateStack");
                GameObject DirtyGlassStackObject = GameObject.Find("DirtyGlassStack");
                GameObject DLC_08DirtyTrayStackObject = GameObject.Find("DLC08_DirtyTrayStack");
                GameObject DirtyMugStackObject = GameObject.Find("DirtyMugStack");

                ServerDirtyPlateStack serverDirtyPlateStack = DirtyPlateStackObject?.GetComponent<ServerDirtyPlateStack>();
                serverDirtyPlateStack?.AddToStack();
                ServerDirtyPlateStack serverDirtyGlassStack = DirtyGlassStackObject?.GetComponent<ServerDirtyPlateStack>();
                serverDirtyGlassStack?.AddToStack();
                ServerDirtyPlateStack serverDLC_08DirtyTrayStack = DLC_08DirtyTrayStackObject?.GetComponent<ServerDirtyPlateStack>();
                serverDLC_08DirtyTrayStack?.AddToStack();
                ServerDirtyPlateStack serverDirtyMugStack = DirtyMugStackObject?.GetComponent<ServerDirtyPlateStack>();
                serverDirtyMugStack?.AddToStack();


                if (DirtyPlateStackObject == null && DirtyGlassStackObject == null && DirtyMugStackObject == null && DLC_08DirtyTrayStackObject == null)
                {
                    //MODEntry.ShowWarningDialog("请先上一个菜, 出“脏盘子/脏杯子/脏托盘/脏马克杯”后再按。无脏盘关无法使用。");
                }
            }
        }
    }
}
