using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class WaveModifierRewardDef : RewardDef
    {
        public int delayBy;
        public float weakenBy;
        public bool allies;

        /*public override string ToStringHuman()
        {
            string desc = "";

            if (delayBy > 0) desc += "VESWW.Delay".Translate(this.delayBy);
            if (weakenBy > 0) desc += "VESWW.Weaken".Translate(this.weakenBy.ToStringPercent());
            if (allies) desc += "VESWW.Allies".Translate();

            return desc.TrimEndNewlines();
        }*/
    }
}
