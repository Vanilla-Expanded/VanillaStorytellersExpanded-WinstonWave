using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VSEWW
{
    public class StorytellerCompProperties_ClassicIntroNoRaid : StorytellerCompProperties
    {
        public StorytellerCompProperties_ClassicIntroNoRaid() => compClass = typeof(StorytellerComp_ClassicIntroNoRaid);
    }

    public class StorytellerComp_ClassicIntroNoRaid : StorytellerComp
    {
        protected int IntervalsPassed => Find.TickManager.TicksGame / 1000;

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            StorytellerComp_ClassicIntroNoRaid compCINR = this;
            if (target == Find.Maps.Find(x => x.IsPlayerHome))
            {
                if (compCINR.IntervalsPassed == 150)
                {
                    if (IncidentDefOf.VisitorGroup.TargetAllowed(target))
                    {
                        yield return new FiringIncident(IncidentDefOf.VisitorGroup, compCINR)
                        {
                            parms = {
                                target = target,
                                points = Rand.Range(40, 100)
                            }
                        };
                    }
                }
                if (compCINR.IntervalsPassed == 204 && DefDatabase<IncidentDef>.AllDefs.Where(def => def.TargetAllowed(target) && def.category == (Find.Storyteller.difficulty.allowIntroThreats ? IncidentCategoryDefOf.ThreatSmall : IncidentCategoryDefOf.Misc))
                        .TryRandomElementByWeight(compCINR.IncidentChanceFinal, out IncidentDef result))
                {
                    yield return new FiringIncident(result, compCINR)
                    {
                        parms = StorytellerUtility.DefaultParmsNow(result.category, target)
                    };
                }
                if (compCINR.IntervalsPassed == 264 && DefDatabase<IncidentDef>.AllDefs.Where(def => def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc)
                    .TryRandomElementByWeight(compCINR.IncidentChanceFinal, out IncidentDef result1))
                {
                    yield return new FiringIncident(result1, compCINR)
                    {
                        parms = StorytellerUtility.DefaultParmsNow(result1.category, target)
                    };
                }
            }
        }
    }
}