using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static Dictionary<Pawn, bool> hediffCache = new Dictionary<Pawn, bool>();

        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.VESWinstonWave");
            harmonyInstance.PatchAll();
        }
    }
}
