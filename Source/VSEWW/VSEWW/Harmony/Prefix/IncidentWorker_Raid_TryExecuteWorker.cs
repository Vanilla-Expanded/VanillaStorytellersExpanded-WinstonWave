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
                Map map = (Map)parms.target;
                if (map.GetComponent<MapComponent_Winston>() is MapComponent_Winston mapComp && mapComp != null)
                {
                    if (mapComp.nextRaidInfo.incidentParms.pawnGroupMakerSeed == parms.pawnGroupMakerSeed || mapComp.nextRaidInfo.reinforcementSeed == parms.pawnGroupMakerSeed)
                        return true;
                }

                if ((parms.faction != null && !parms.faction.HostileTo(Faction.OfPlayer)) || parms.quest != null) // Not hostile to player or quest raid
                {
                    return true; // Send it
                }

                Log.Warning($"[VSEWW] Prevented raid");
                return false;
            }
            return true;
        }
    }
}