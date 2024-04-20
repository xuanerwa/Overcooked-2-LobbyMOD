using BepInEx.Configuration;
using UnityEngine;

namespace HostUtilities
{
    public class AddCleanDishes
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> AddCleanDishesKey;
        public static int startTime;
        public static bool cooling = false;
        public static int plateOrGlassNum = 0;

        public static void Awake()
        {
            AddCleanDishesKey = _MODEntry.Instance.Config.Bind("01-按键绑定(仅街机)", "13-增加1个盘子/杯子/托盘", KeyCode.Alpha0);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(AddCleanDishesKey.Value))
            {
                if (!_MODEntry.IsInParty)
                {
                    _MODEntry.ShowWarningDialog("请在街机中使用此功能。");
                    return;
                }
                if (plateOrGlassNum >= 5)
                {
                    _MODEntry.ShowWarningDialog("达到最大数量, 别点啦。");
                    return;
                }

                //_MODEntry.LogInfo("增加盘子(必须先洗一个脏盘子)");
                GameObject cleanPlateStackObject = GameObject.Find("CleanPlateStack");
                GameObject cleanGlassStackObject = GameObject.Find("CleanGlassStack");
                GameObject DLC_08CleanTrayStackObject = GameObject.Find("DLC08_CleanTrayStack");
                if (cleanPlateStackObject != null)
                {

                    // 获取 cleanPlateStackObject 对象上的 ServerCleanPlateStack 组件
                    ServerCleanPlateStack serverCleanPlateStack = cleanPlateStackObject.GetComponent<ServerCleanPlateStack>();
                    if (serverCleanPlateStack != null)
                    {
                        // 调用 ServerCleanPlateStack 组件的 AddToStack 方法
                        serverCleanPlateStack.AddToStack();
                        plateOrGlassNum += 1;
                    }
                }

                if (cleanGlassStackObject != null)
                {
                    // 获取 cleanGlassStackObject 对象上的 ServerCleanPlateStack 组件
                    ServerCleanPlateStack serverCleanGlassStack = cleanGlassStackObject.GetComponent<ServerCleanPlateStack>();
                    if (serverCleanGlassStack != null)
                    {
                        // 调用 serverCleanGlassStack 组件的 AddToStack 方法
                        serverCleanGlassStack.AddToStack();
                        plateOrGlassNum += 1;
                    }
                }
                if (DLC_08CleanTrayStackObject != null)
                {

                    // 获取 DLC_08CleanTrayStackObject 对象上的 ServerCleanPlateStack 组件
                    ServerCleanPlateStack serverDLC_08CleanTrayStack = DLC_08CleanTrayStackObject.GetComponent<ServerCleanPlateStack>();
                    if (serverDLC_08CleanTrayStack != null)
                    {
                        // 调用 ServerDLC_08CleanTrayStack 组件的 AddToStack 方法
                        serverDLC_08CleanTrayStack.AddToStack();
                        plateOrGlassNum += 1;
                    }
                }
                if (cleanGlassStackObject != null && cleanPlateStackObject != null)
                {
                    plateOrGlassNum -= 1;
                }

                if (cleanPlateStackObject == null && cleanGlassStackObject == null)
                {
                    _MODEntry.ShowWarningDialog("请先洗一个盘子/杯子/托盘后再按。");
                }

            }
        }
    }
}
