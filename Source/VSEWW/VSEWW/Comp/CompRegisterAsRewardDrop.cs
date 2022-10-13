using RimWorld;
using Verse;

namespace VSEWW
{
    internal class CompRegisterAsRewardDrop : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var dropSpots = parent.Map.listerBuildings.AllBuildingsColonistOfDef(parent.def);

            foreach (var spot in dropSpots)
            {
                if (spot != parent)
                {
                    Messages.Message("VESWW.RemoveOldDropSpot".Translate(), MessageTypeDefOf.NeutralEvent, false);
                    spot.DeSpawn();
                }
            }

            parent.Map.GetComponent<MapComponent_Winston>()?.RegisterDropSpot(parent.Position);
        }
    }
}