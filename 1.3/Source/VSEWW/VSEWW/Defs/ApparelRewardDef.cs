using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class ApparelReward
    {
        public string apparelTag;
        public TechLevel maxTechLevel;
        public QualityCategory quality;
        public int count;
    }

    public class ApparelRewardDef : RewardDef
    {
        public List<ApparelReward> apparels;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";

            foreach (ApparelReward wp in apparels.Distinct())
            {
                desc += $"- {"VESWW.RandomApparel".Translate(wp.count, wp.quality.ToString(), wp.apparelTag)}\n";
            }

            return desc.TrimEndNewlines();
        }
    }
}
