using UnityEngine;
using Verse;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        internal static readonly Texture2D WaveBGTex = ContentFinder<Texture2D>.Get("UI/Waves/WaveBG");
        internal static readonly Texture2D ModifierBGTex = ContentFinder<Texture2D>.Get("UI/Modifiers/ModifierBG");
        internal static readonly Texture2D NormalTex = ContentFinder<Texture2D>.Get("UI/Waves/Wave_Normal");
        internal static readonly Texture2D BossTex = ContentFinder<Texture2D>.Get("UI/Waves/Wave_Boss");
    }
}
