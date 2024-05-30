using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace HostUtilities
{
    public class ReplaceOneShotAudio
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);

        public static Harmony HarmonyInstance { get; set; }
        public static ConfigEntry<string> audioPackSelected;
        private static string[] audioPackList;
        public static void Awake()
        {
            audioPackList = GetAudioPackList();
            audioPackSelected = MODEntry.Instance.Config.Bind<string>("01-功能开关", "05-选择语音包", audioPackList[0], new ConfigDescription("选择语音包", new AcceptableValueList<string>(audioPackList)));
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;

        }


        public static string[] GetAudioPackList()
        {
            string dllFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioFolderPath = Path.Combine(dllFolderPath, "Audio");
            // 确保音频文件夹存在
            if (!Directory.Exists(audioFolderPath))
            {
                return new string[] { "不替换" };
            }

            string[] fileNames = Directory.GetFiles(audioFolderPath)
                                           .Select(Path.GetFileName)
                                           .ToArray();

            string[] audioPackNames = fileNames.Where(fileName => fileName != "添加新语音方法.txt")
                                               .Select(fileName => fileName.Split('-')[0].Trim())
                                               .ToArray();
            audioPackNames = audioPackNames.Distinct().ToArray();
            // 将 "不替换" 添加到数组的开头
            string[] result = new string[audioPackNames.Length + 1];
            result[0] = "不替换";
            Array.Copy(audioPackNames, 0, result, 1, audioPackNames.Length);
            return result;
        }


        [HarmonyPatch(typeof(AudioManager), "FindEntry", new System.Type[] { typeof(GameOneShotAudioTag) })]
        [HarmonyPostfix]
        public static void FindEntryPostfix(ref AudioDirectoryEntry __result, GameOneShotAudioTag _tag)
        {
            if (audioPackSelected.Value != "不替换")
            {
                if (_tag == GameOneShotAudioTag.Curse || _tag == GameOneShotAudioTag.UIEmoteSwear)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-骂人");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteOk)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-好");
                }
                else if (_tag == GameOneShotAudioTag.UIEmotePrep)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-准备中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteServing)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-上菜中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteWashUp)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-清理中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteCooking)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-烹饪中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteDisappointed)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-祝你下次好运");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteGreatJob)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-干得漂亮");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteHi)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-你好");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteCelebrate)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-我们成功啦");
                }
                else if (_tag == GameOneShotAudioTag.Chop)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-切");
                }
                else if (_tag == GameOneShotAudioTag.Throw)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-丢");
                }
                else if (_tag == GameOneShotAudioTag.Pickup)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-拿");
                }
                else if (_tag == GameOneShotAudioTag.PutDown)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-放");
                }
                else if (_tag == GameOneShotAudioTag.Catch)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-接");
                }
                else if (_tag == GameOneShotAudioTag.Dash)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-冲刺");
                }
                else if (_tag == GameOneShotAudioTag.TimesUp)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-时间到");
                }
                else if (_tag == GameOneShotAudioTag.TrashCan)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-垃圾桶");
                }
                else if (_tag == GameOneShotAudioTag.RecipeTimeOut)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-菜单过期");
                }
                else if (_tag == GameOneShotAudioTag.UIChefSelected)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-选中厨师");
                }
                else if (_tag == GameOneShotAudioTag.WashedPlate)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-出盘");
                }
                else if (_tag == GameOneShotAudioTag.WashingSplash)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-洗盘");
                }
                else if (_tag == GameOneShotAudioTag.SuccessfulDelivery)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-上菜成功");
                }
                else if (_tag == GameOneShotAudioTag.ImCooked)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-锅熟");
                }
                else if (_tag == GameOneShotAudioTag.CookingWarning)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-锅叫");
                }
                else if (_tag == GameOneShotAudioTag.FireIgnition)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-着火");
                }
                else if (_tag == GameOneShotAudioTag.Impact)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-碰撞");
                }
                else if (_tag == GameOneShotAudioTag.DLC_07_Failed)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-厨房被毁");
                }
                else if (_tag == GameOneShotAudioTag.DLC_07_Hammer)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-修门");
                }
                else if (_tag == GameOneShotAudioTag.DLC_08_Cannon_Fire)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-大炮发射");
                }
                else if (_tag == GameOneShotAudioTag.ResultsStar01)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-一星");
                }
                else if (_tag == GameOneShotAudioTag.ResultsStar02)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-两星");
                }
                else if (_tag == GameOneShotAudioTag.ResultsStar03)
                {
                    LoadTagFile(ref __result, $"{audioPackSelected.Value}-三星");
                }
            }
        }

        private static void LoadTagFile(ref AudioDirectoryEntry __result, string _tag)
        {
            string dllFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioFolderPath = Path.Combine(dllFolderPath, "Audio");
            string[] wavFiles = System.IO.Directory.GetFiles(audioFolderPath, $"{_tag}-*.wav");

            if (wavFiles.Length > 0)
            {
                //随机选择一个wav
                int randomIndex = UnityEngine.Random.Range(0, wavFiles.Length);
                string wavFilePath = wavFiles[randomIndex];

                // 加载新的AudioClip
                AudioClip newAudioClip = LoadWavFile(wavFilePath);

                if (newAudioClip != null)
                {
                    // 设置新的AudioClip
                    AudioDirectoryData.OneShotAudioDirectoryEntry m_findEntryList = new AudioDirectoryData.OneShotAudioDirectoryEntry();
                    m_findEntryList.AudioFile = newAudioClip;
                    __result = m_findEntryList;
                }
                else
                {
                    LogE("读取wav失败");
                }
            }
            else
            {
                //LogE($"文件夹内没有 {_tag.ToString()}-*.wav 文件");
            }
        }



        private static AudioClip LoadWavFile(string filePath)
        {
            // 使用UnityWebRequest加载WAV文件
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV);

            // 发送请求
            www.SendWebRequest();

            // 等待请求完成
            while (!www.isDone) { }

            // 获取AudioClip
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

            return audioClip;

        }


    }
}