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
            var comp = this;
            if (target == Find.Maps.Find(x => x.IsPlayerHome))
            {
                if (comp.IntervalsPassed == 150)
                {
                    if (IncidentDefOf.VisitorGroup.TargetAllowed(target))
                    {
                        yield return new FiringIncident(IncidentDefOf.VisitorGroup, comp)
                        {
                            parms = {
                                target = target,
                                points = Rand.Range(40, 100)
                            }
                        };
                    }
                }

                if (comp.IntervalsPassed == 204)
                {
                    IncidentCategoryDef threatCategory = Find.Storyteller.difficulty.allowIntroThreats ? IncidentCategoryDefOf.ThreatSmall : IncidentCategoryDefOf.Misc;
                    if (DefDatabase<IncidentDef>.AllDefs.Where(def => def.TargetAllowed(target) && def.category == threatCategory).TryRandomElementByWeight((IncidentDef x) => comp.IncidentChanceFinal(x,target), out IncidentDef result))
                    {
                        yield return new FiringIncident(result, comp)
                        {
                            parms = StorytellerUtility.DefaultParmsNow(result.category, target)
                        };
                    }
                }

                if (comp.IntervalsPassed == 264 && DefDatabase<IncidentDef>.AllDefs.Where(def => def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc).TryRandomElementByWeight((IncidentDef x) => comp.IncidentChanceFinal(x, target), out IncidentDef incident))
                {
                    yield return new FiringIncident(incident, comp)
                    {
                        parms = StorytellerUtility.DefaultParmsNow(incident.category, target)
                    };
                }
            }
        }
    }
}