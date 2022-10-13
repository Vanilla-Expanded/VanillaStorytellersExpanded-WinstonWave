using HarmonyLib;
using RimWorld;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(TaleUtility))]
    [HarmonyPatch("Notify_PawnDied", MethodType.Normal)]
    public class TaleUtility_Notify_PawnDied
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn victim)
        {
            if (victim.IsColonist)
            {
                int kills = (int)(victim.records.GetValue(RecordDefOf.KillsHumanlikes) + victim.records.GetValue(RecordDefOf.KillsMechanoids));
                Find.World.GetComponent<WorldComponent_KillCounter>().AddKills(kills);
            }
        }
    }
}