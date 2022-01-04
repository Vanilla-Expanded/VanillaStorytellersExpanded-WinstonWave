using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(IncidentWorker_Raid))]
    [HarmonyPatch("TryGenerateRaidInfo", MethodType.Normal)]
    public class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(IncidentParms parms, out List<Pawn> pawns, ref IncidentWorker_Raid __instance, ref bool __result)
        {
            pawns = null;
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                Map map = (Map)parms.target;
                if (map.GetComponent<MapComponent_Winston>() is MapComponent_Winston mapComp && mapComp != null)
                {
                    __instance.ResolveRaidStrategy(parms, PawnGroupKindDefOf.Combat);
                    __instance.ResolveRaidArriveMode(parms);

                    if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
                    {
                        Log.Error($"[VESWW] Couldn't reslove raid spawn center");
                        __result = false;
                        return false;
                    }

                    pawns = mapComp.nextRaidInfo.raidPawns;

                    if (pawns.Count == 0)
                    {
                        Log.Error($"[VESWW] Tried to use empty raiders list");
                        __result = false;
                        return false;
                    }
                    parms.raidArrivalMode.Worker.Arrive(pawns, parms);
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}
