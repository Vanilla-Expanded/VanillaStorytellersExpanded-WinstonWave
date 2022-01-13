using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    internal class CompRegisterAsRewardDrop : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var dropSpots = parent.Map.listerBuildings.AllBuildingsColonistOfDef(parent.def).ToList();
            int count = dropSpots.Count;
            if (count > 0)
            {
                Messages.Message("VESWW.RemoveOldDropSpot".Translate(), MessageTypeDefOf.NeutralEvent, false);
                for (int i = 0; i < count; i++)
                {
                    dropSpots[0].DeSpawn();
                    dropSpots.RemoveAt(0);
                }
            }
            parent.Map.GetComponent<MapComponent_Winston>()?.RegisterDropSpot(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            parent.Map.GetComponent<MapComponent_Winston>()?.UnRegisterDropSpot();
        }
    }
}
