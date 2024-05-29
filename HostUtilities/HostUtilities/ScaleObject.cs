using BepInEx.Configuration;
using UnityEngine;

namespace HostUtilities
{
    public class ScaleObject
    {
        public static void log(string mes) => _MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> zoomOutKey;
        public static ConfigEntry<KeyCode> magnifyKey;
        public static GameObject parentObject = null;

        public static void Awake()
        {
            zoomOutKey = _MODEntry.Instance.Config.Bind("02-按键绑定", "14-放大手上拿的物体", KeyCode.Equals, "退格左边的 +/= 键");
            magnifyKey = _MODEntry.Instance.Config.Bind("02-按键绑定", "15-缩小手上拿的物体", KeyCode.Minus, "Alpha0右边的-/_键");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(zoomOutKey.Value))
            {
                GameObject foundObject;
                foreach (string playerindex in new string[] { "Player 1", "Player 2", "Player 3", "Player 4" })
                {
                    foundObject = FindAttachmentUnderPlayer("Chefs", playerindex, "Chef/Skeleton/Base/Attach/Attachment");

                    if (parentObject == null || parentObject != foundObject)
                        parentObject = foundObject;

                    if (parentObject != null)
                    {
                        foreach (Transform child in parentObject.transform)
                        {
                            Debug.Log($"Object find: {child.name}");
                            ScaleObjectAndChildren(child, new Vector3(0.4f, 0.4f, 0.4f));
                        }
                    }
                    else
                    {
                        Debug.LogError("Attachment not found under " + playerindex);
                    }
                }
            }

            if (Input.GetKeyDown(magnifyKey.Value))
            {
                GameObject foundObject;
                foreach (string playerindex in new string[] { "Player 1", "Player 2", "Player 3", "Player 4" })
                {
                    foundObject = FindAttachmentUnderPlayer("Chefs", playerindex, "Chef/Skeleton/Base/Attach/Attachment");

                    if (parentObject == null || parentObject != foundObject)
                        parentObject = foundObject;

                    if (parentObject != null)
                    {
                        foreach (Transform child in parentObject.transform)
                        {
                            Debug.Log($"Object find: {child.name}");
                            ScaleObjectAndChildren(child, new Vector3(-0.4f, -0.4f, -0.4f));
                        }
                    }
                    else
                    {
                        Debug.LogError("Attachment not found under " + playerindex);
                    }
                }
            }
        }

        static GameObject FindAttachmentUnderPlayer(string parentName, string playerName, string attachmentName)
        {
            // 先在指定的父对象（如 Chefs）下寻找 Player 1
            GameObject parentObj = GameObject.Find(parentName);
            if (parentObj != null)
            {
                Transform player = parentObj.transform.Find(playerName);
                if (player != null)
                {
                    Transform attachment = player.Find(attachmentName);
                    if (attachment != null)
                    {
                        return attachment.gameObject;
                    }
                }
            }

            // 如果在指定父对象下未找到，再遍历整个场景寻找 Player 1
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == playerName)
                {
                    Transform attachment = obj.transform.Find(attachmentName);
                    if (attachment != null)
                    {
                        return attachment.gameObject;
                    }
                }
            }
            return null;
        }

        static void ScaleObjectAndChildren(Transform obj, Vector3 scale)
        {
            if (obj.childCount == 0)
            {
                // 如果没有子对象，缩放它
                obj.localScale += scale;
                Debug.Log($"Scaled object: {obj.name}");
            }
            else
            {
                // 如果有子对象，递归遍历子对象
                foreach (Transform child in obj)
                {
                    ScaleObjectAndChildren(child, scale);
                }
            }
        }
    }
}
