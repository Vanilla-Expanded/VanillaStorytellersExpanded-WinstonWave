using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class IncidentRewardDef : RewardDef
    {
        public IncidentDef incidentDef;

        /*public override string ToStringHuman()
        {
            string desc = $"- {"VESWW.FiringEvent".Translate(incidentDef.label)}\n";

            return desc.TrimEndNewlines();
        }*/
    }
}
