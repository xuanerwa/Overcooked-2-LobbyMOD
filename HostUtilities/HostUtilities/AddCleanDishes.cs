using BepInEx.Configuration;
using UnityEngine;

namespace HostUtilities
{
    public class AddCleanDishes
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> AddDirtyDishesKey;
        public static int startTime;
        public static bool cooling = false;

        public static void Awake()
        {
            AddDirtyDishesKey = _MODEntry.Instance.Config.Bind("01-按键绑定(仅街机)", "13-增加10个盘子", KeyCode.Alpha0, "增加盘子");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(AddDirtyDishesKey.Value))
            {
                if (!_MODEntry.IsInParty)
                {
                    _MODEntry.ShowWarningDialog("请在街机游戏中使用此功能。");
                    return;
                }
                //_MODEntry.LogInfo("增加盘子(必须先洗一个脏盘子)");
                GameObject dirtyPlateStackObject = GameObject.Find("CleanPlateStack");
                if (dirtyPlateStackObject != null)
                {

                    // 获取 CleanPlateStack 对象上的 ServerCleanPlateStack 组件
                    ServerCleanPlateStack ServerCleanPlateStack = dirtyPlateStackObject.GetComponent<ServerCleanPlateStack>();
                    if (ServerCleanPlateStack != null)
                    {
                        // 调用 ServerCleanPlateStack 组件的 AddToStack 方法
                        for (int i = 0; i < 10; i++)
                        {
                            ServerCleanPlateStack.AddToStack();
                        }
                    }
                }
                else
                {
                    _MODEntry.ShowWarningDialog("请先洗一个盘子后再按。");
                }

            }
        }
    }
}
