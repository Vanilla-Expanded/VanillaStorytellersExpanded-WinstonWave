using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class RCategoryRewardDef : RewardDef
    {
        public RewardCategory rewardOf;

        /*public override string ToStringHuman()
        {
            string desc = "VESWW.RandomRewardOf".Translate(this.rewardOf.ToString());

            return desc;
        }*/
    }
}
