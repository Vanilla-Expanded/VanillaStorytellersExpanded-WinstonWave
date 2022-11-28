using HarmonyLib;
using RimWorld;
using Verse;
namespace VSEWW
{
    [HarmonyPatch(typeof(IncidentWorker_Raid))]
    [HarmonyPatch("TryExecuteWorker", MethodType.Normal)]
    public class IncidentWorker_Raid_TryExecuteWorker
    {
        [HarmonyPrefix]
        public static bool Prefix(IncidentParms parms, ref IncidentWorker_Raid __instance)
        {
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                // Only send friendly or quest raid
                if ((parms.faction != null && !parms.faction.HostileTo(Faction.OfPlayer)) || parms.quest != null)
                    return true;

                Log.Warning($"[VSEWW] Prevented raid");
                return false;
            }
            return true;
        }
    }
}