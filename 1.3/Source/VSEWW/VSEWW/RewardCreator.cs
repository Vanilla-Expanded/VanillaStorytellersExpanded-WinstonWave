using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VSEWW
{
    public static class RewardCreator
    {
        public static void SendReward(RewardDef reward, Map map)
        {
            if (reward is IncidentRewardDef incidentRewardDef)
            {
                Find.Storyteller.incidentQueue.Add(incidentRewardDef.incidentDef, Find.TickManager.TicksGame, new IncidentParms
                {
                    target = map
                });
            }
            else
            {
                List<Thing> thingList = CreateThingListFromRewardDef(reward);
                IntVec3 intVec3 = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.DropPodIncoming.Size, map.ParentFaction);

                Log.Message($"Sending {thingList.Count} things to {intVec3}");
                DropPodUtility.DropThingsNear(intVec3, map, thingList, leaveSlag: true);
            }
        }

        private static List<Thing> CreateThingListFromRewardDef(RewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            if (reward is BlockRewardDef) things.AddRange(FromBlockRewardDef(reward));

            return things;
        }

        private static List<Thing> FromBlockRewardDef(RewardDef reward)
        {
            List<Thing> things = new List<Thing>();
            BlockRewardDef blockRewardDef = reward as BlockRewardDef;

            ThingDef blockDef = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.thingCategories != null && t.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks)).RandomElement();
            int countLeft = blockRewardDef.count;
            while (countLeft > 0)
            {
                Thing block = ThingMaker.MakeThing(blockDef);
                int stack = Math.Min(countLeft, blockDef.stackLimit);
                block.stackCount = stack;
                countLeft -= stack;
                things.Add(block);
            }

            return things;
        }
    }
}
