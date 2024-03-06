using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace LobbyMODS
{
    public class UnlockChefs
    {
        public static Harmony HarmonyInstance { get; set; }
        public static void Awake()
        {
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        [HarmonyPatch(typeof(MetaGameProgress), nameof(MetaGameProgress.GetUnlockedAvatars))]
        [HarmonyPrefix]
        private static bool GetUnlockedAvatars(ref AvatarDirectoryData[] ___m_allAvatarDirectories, ref ChefAvatarData[] __result)
        {
            List<ChefAvatarData> list = new List<ChefAvatarData>();

            for (int i = 0; i < ___m_allAvatarDirectories.Length; i++)
            {
                AvatarDirectoryData avatarDirectoryData = ___m_allAvatarDirectories[i];
                for (int j = 0; j < avatarDirectoryData.Avatars.Length; j++)
                {
                    ChefAvatarData chefAvatarData = avatarDirectoryData.Avatars[j];
                    if (chefAvatarData != null)
                    {
                        list.Add(chefAvatarData);
                    }
                }
            }

            __result = list.ToArray();
            return false;
        }

        [HarmonyPatch(typeof(ScoreScreenOutroFlowroutine), "Run")]
        [HarmonyPrefix]
        private static void Run(ref ScoreScreenFlowroutineData ___m_flowroutineData)
        {
            ___m_flowroutineData.m_unlocks = new GameProgress.UnlockData[] { };
        }
    }
}

