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

        static Startup()
        {
            weatherDecider_ticksWhenRainAllowedAgain = typeof(WeatherDecider).GetField("ticksWhenRainAllowedAgain", BindingFlags.NonPublic | BindingFlags.Instance);

            CEActive = ModsConfig.IsActive("CETeam.CombatExtended");
            NoPauseChallengeActive = ModsConfig.IsActive("brrainz.nopausechallenge");

            counterColor = new Color
            {
                r = Widgets.WindowBGFillColor.r,
                g = Widgets.WindowBGFillColor.g,
                b = Widgets.WindowBGFillColor.b,
                a = 0.25f
            };
        }
    }
}