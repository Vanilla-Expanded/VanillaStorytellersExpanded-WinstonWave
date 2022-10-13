using HarmonyLib;
using RimWorld;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(RecordsUtility))]
    [HarmonyPatch("Notify_PawnKilled", MethodType.Normal)]
    public class RecordsUtility_Notify_PawnKilled
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn killed, Pawn killer)
        {
            // Only if killer is a colonist
            if (killer.Faction != Faction.OfPlayer)
                return;

            var raceProps = killed.RaceProps;
            if (raceProps.Humanlike || raceProps.IsMechanoid)
                Find.World.GetComponent<WorldComponent_KillCounter>().IncrementCounter();
        }
    }
}