using BepInEx.Configuration;
using UnityEngine;

namespace HostUtilities
{
    public class AddDirtyDishes
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> AddDirtyDishesKey;
        public static int startTime;
        public static bool cooling = false;
        public static int plateOrGlassNum = 0;

        public static void Awake()
        {
            AddDirtyDishesKey = _MODEntry.Instance.Config.Bind("01-按键绑定(仅街机)", "13-增加1个脏盘/杯", KeyCode.Alpha0);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(AddDirtyDishesKey.Value))
            {
                if (!_MODEntry.IsInParty)
                {
                    _MODEntry.ShowWarningDialog("请在街机中使用此功能。");
                    return;
                }
                //if (plateOrGlassNum >= 5)
                //{
                //    _MODEntry.ShowWarningDialog("达到最大数量, 别点啦。");
                //    return;
                //}

                //_MODEntry.LogInfo("增加盘子(必须先洗一个脏盘子)");
                GameObject DirtyPlateStackObject = GameObject.Find("DirtyPlateStack");
                GameObject DirtyGlassStackObject = GameObject.Find("DirtyGlassStack");
                GameObject DLC_08DirtyTrayStackObject = GameObject.Find("DLC08_DirtyTrayStack");
                GameObject DirtyMugStackObject = GameObject.Find("DirtyMugStack");
                if (DirtyPlateStackObject != null)
                {
                    //盘子
                    // 获取 DirtyPlateStackObject 对象上的 ServerDirtyPlateStack 组件
                    ServerDirtyPlateStack serverDirtyPlateStack = DirtyPlateStackObject.GetComponent<ServerDirtyPlateStack>();
                    if (serverDirtyPlateStack != null)
                    {
                        // 调用 ServerDirtyPlateStack 组件的 AddToStack 方法
                        serverDirtyPlateStack.AddToStack();
                    }
                }

                if (DirtyGlassStackObject != null)
                {
                    //杯子
                    // 获取 DirtyGlassStackObject 对象上的 ServerDirtyPlateStack 组件
                    ServerDirtyPlateStack serverDirtyGlassStack = DirtyGlassStackObject.GetComponent<ServerDirtyPlateStack>();
                    if (serverDirtyGlassStack != null)
                    {
                        // 调用 serverDirtyGlassStack 组件的 AddToStack 方法
                        serverDirtyGlassStack.AddToStack();
                    }
                }

                if (DLC_08DirtyTrayStackObject != null)
                {
                    //托盘
                    // 获取 DLC_08DirtyTrayStackObject 对象上的 ServerDirtyPlateStack 组件
                    ServerDirtyPlateStack serverDLC_08DirtyTrayStack = DLC_08DirtyTrayStackObject.GetComponent<ServerDirtyPlateStack>();
                    if (serverDLC_08DirtyTrayStack != null)
                    {
                        // 调用 ServerDLC_08DirtyTrayStack 组件的 AddToStack 方法
                        serverDLC_08DirtyTrayStack.AddToStack();
                    }
                }
                if (DirtyMugStackObject != null)
                {
                    // 马克杯
                    // 获取 DirtyMugStackObject 对象上的 ServerDirtyPlateStack 组件
                    ServerDirtyPlateStack serverDirtyMugStack = DirtyMugStackObject.GetComponent<ServerDirtyPlateStack>();
                    if (serverDirtyMugStack != null)
                    {
                        // 调用 serverDirtyMugStack 组件的 AddToStack 方法
                        serverDirtyMugStack.AddToStack();
                    }
                }
                //if (DirtyGlassStackObject != null && DirtyPlateStackObject != null)
                //{
                //    plateOrGlassNum -= 1;
                //}

                if (DirtyPlateStackObject == null && DirtyGlassStackObject == null && DirtyMugStackObject == null && DLC_08DirtyTrayStackObject == null)
                {
                    _MODEntry.ShowWarningDialog("请先上一个菜, 出“脏盘子/脏杯子/脏托盘/脏马克杯”后再按。");
                }

            }
        }
    }
}
