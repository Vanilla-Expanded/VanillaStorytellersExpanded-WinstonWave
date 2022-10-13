using RimWorld;
using Verse;

namespace VSEWW
{
    internal class CompRegisterAsRewardDrop : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            var map = parent.Map;
            var dropSpots = map.listerBuildings.AllBuildingsColonistOfDef(parent.def);

            foreach (var spot in dropSpots)
            {
                if (spot != parent)
                {
                    Messages.Message("VESWW.RemoveOldDropSpot".Translate(), MessageTypeDefOf.NeutralEvent, false);
                    spot.DeSpawn();
                }
            }

            map.GetComponent<MapComponent_Winston>()?.RegisterDropSpot(parent.Position);
        }
    }
}