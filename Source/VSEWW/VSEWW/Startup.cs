using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        internal static readonly Texture2D WaveBGTex = ContentFinder<Texture2D>.Get("UI/Waves/WaveBG");
        internal static readonly Texture2D ModifierBGTex = ContentFinder<Texture2D>.Get("UI/Modifiers/ModifierBG");
        internal static readonly Texture2D NormalTex = ContentFinder<Texture2D>.Get("UI/Waves/Wave_Normal");
        internal static readonly Texture2D BossTex = ContentFinder<Texture2D>.Get("UI/Waves/Wave_Boss");

        internal static Dictionary<Pawn, bool> hediffCache = new Dictionary<Pawn, bool>();

        internal static FieldInfo weatherDecider_ticksWhenRainAllowedAgain;

        internal static bool CEActive = false;
        internal static bool NoPauseChallengeActive = false;

        internal static Color counterColor;

        internal static Dictionary<RewardCategory, List<RewardDef>> rewardsPerCat;

        static Startup()
        {
            // Cache field info
            weatherDecider_ticksWhenRainAllowedAgain = typeof(WeatherDecider).GetField("ticksWhenRainAllowedAgain", BindingFlags.NonPublic | BindingFlags.Instance);
            // Mod active check
            CEActive = ModsConfig.IsActive("CETeam.CombatExtended");
            NoPauseChallengeActive = ModsConfig.IsActive("brrainz.nopausechallenge");
            // Create color
            counterColor = new Color
            {
                r = Widgets.WindowBGFillColor.r,
                g = Widgets.WindowBGFillColor.g,
                b = Widgets.WindowBGFillColor.b,
                a = 0.25f
            };
            // Populate dictionnary
            rewardsPerCat = new Dictionary<RewardCategory, List<RewardDef>>
            {
                { RewardCategory.Poor, new List<RewardDef>() },
                { RewardCategory.Normal, new List<RewardDef>() },
                { RewardCategory.Good, new List<RewardDef>() },
                { RewardCategory.Excellent, new List<RewardDef>() },
                { RewardCategory.Legendary, new List<RewardDef>() }
            };

            var rewards = DefDatabase<RewardDef>.AllDefsListForReading;
            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                rewardsPerCat[reward.category].Add(reward);
            }
        }
    }
}