using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class ReseachRewardDef : RewardDef
    {
        public int count;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";
            desc += "VESWW.Research".Translate(count);

            return desc;
        }
    }
}
