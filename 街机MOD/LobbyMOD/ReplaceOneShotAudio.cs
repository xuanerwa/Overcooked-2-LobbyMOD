using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using static OptionsData;

namespace LobbyMODS
{
    public class ReplaceOneShotAudio : BaseUnityPlugin
    {
        private static ConfigEntry<bool> isEnabled;
        public static void Awake()
        {
            isEnabled = MODEntry.Instance.Config.Bind<bool>("00-功能开关", "替换表情语音", false);
            Harmony.CreateAndPatchAll(typeof(ReplaceOneShotAudio));
        }

        [HarmonyPatch(typeof(AudioManager), "FindEntry", new System.Type[] { typeof(GameOneShotAudioTag) })]
        [HarmonyPostfix]
        public static void FindEntryPostfix(ref AudioDirectoryEntry __result, GameOneShotAudioTag _tag)
        {
            if (isEnabled.Value)
            {
                //MODEntry.LogError("替换骂人语音");
                if (_tag == GameOneShotAudioTag.Curse || _tag == GameOneShotAudioTag.UIEmoteSwear)
                {
                    loadTagFile(ref __result, "骂人");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteOk)
                {
                    loadTagFile(ref __result, "好");
                }
                else if (_tag == GameOneShotAudioTag.UIEmotePrep)
                {
                    loadTagFile(ref __result, "准备中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteServing)
                {
                    loadTagFile(ref __result, "上菜中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteWashUp)
                {
                    loadTagFile(ref __result, "清理中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteCooking)
                {
                    loadTagFile(ref __result, "烹饪中");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteDisappointed)
                {
                    loadTagFile(ref __result, "祝你下次好运");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteGreatJob)
                {
                    loadTagFile(ref __result, "干得漂亮");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteHi)
                {
                    loadTagFile(ref __result, "你好");
                }
                else if (_tag == GameOneShotAudioTag.UIEmoteCelebrate)
                {
                    loadTagFile(ref __result, "我们成功啦");
                }
            }

        }

        private static void loadTagFile(ref AudioDirectoryEntry __result, string _tag)
        {
            string dllFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioFolderPath = Path.Combine(dllFolderPath, "Audio");
            string[] wavFiles = System.IO.Directory.GetFiles(audioFolderPath, $"{_tag}*.wav");

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
                    MODEntry.LogError("读取wav失败");
                }
            }
            else
            {
                MODEntry.LogError($"文件夹内没有 {_tag.ToString()}*.wav 文件");
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