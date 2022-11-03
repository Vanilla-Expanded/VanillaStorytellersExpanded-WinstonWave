using HarmonyLib;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("HealthScale", MethodType.Getter)]
    public class Pawn_HealthScale
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, ref Pawn __instance)
        {
            if (Find.Storyteller.def.defName == "VSE_WinstonWave" && __instance.RaceProps.Humanlike)
            {
                if (Startup.hediffCache.ContainsKey(__instance))
                {
                    if (Startup.hediffCache[__instance])
                        __result *= 2;
                }
                else if (__instance.Spawned)
                {
                    Startup.hediffCache[__instance] = __instance.health.hediffSet.HasHediff(WHediffDefOf.VSEWW_BulletSponge);
                }
            }
        }
    }
}