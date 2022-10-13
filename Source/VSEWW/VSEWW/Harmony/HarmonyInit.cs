using HarmonyLib;
using Verse;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.VESWinstonWave");
            harmonyInstance.PatchAll();
        }
    }
}