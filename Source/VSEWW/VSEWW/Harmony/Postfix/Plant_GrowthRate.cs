using HarmonyLib;
using RimWorld;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("GrowthRate", MethodType.Getter)]
    public class Plant_GrowthRate
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (WinstonMod.settings.enableStatIncrease && Find.Storyteller.def.defName == "VSE_WinstonWave")
                __result *= 1.75f;
        }
    }
}