using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VSEWW
{
    public class RPawnReward
    {
        public string tradeTag = "";
        public Intelligence intelligence = Intelligence.Humanlike;
        public int maxCombatPower;
        public int minCombatPower;
        public int count;
    }

    public class RPawnRewardDef : RewardDef
    {
        public List<RPawnReward> randomPawns;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";

            foreach (RPawnReward rp in randomPawns.Distinct())
            {
                desc += $"- {"VESWW.RandomPawns".Translate(rp.count, rp.intelligence.ToString())}\n";
            }

            return desc.TrimEndNewlines();
        }
    }
}
