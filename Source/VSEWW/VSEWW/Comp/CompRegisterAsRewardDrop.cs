using RimWorld;
using System.Linq;
using Verse;

namespace VSEWW
{
    internal class CompRegisterAsRewardDrop : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var dropSpots = parent.Map.listerBuildings.AllBuildingsColonistOfDef(parent.def);

            if (dropSpots.Count() > 0)
            {
                Messages.Message("VESWW.RemoveOldDropSpot".Translate(), MessageTypeDefOf.NeutralEvent, false);
                dropSpots.ElementAt(0).DeSpawn();
            }


            parent.Map.GetComponent<MapComponent_Winston>()?.RegisterDropSpot(parent.Position);
        }
    }
}
