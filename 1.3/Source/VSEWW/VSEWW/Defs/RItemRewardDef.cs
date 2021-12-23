using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class RItemReward
    {
        public List<ThingCategoryDef> thingCategories;
        public QualityCategory quality;
        public int count;
    }

    public class RItemRewardDef : RewardDef
    {
        public List<RItemReward> randomItems;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";

            foreach (RItemReward ri in randomItems.Distinct())
            {
                if (ri.thingCategories.Count > 1)
                {
                    string cats = "";
                    foreach (var item in ri.thingCategories)
                    {
                        cats += item.LabelCap + ",";
                    }

                    desc += $"- {"VESWW.RandomThingMC".Translate(ri.count, ri.quality.ToString(), cats.TrimEnd(new char[] { ','}))}\n";
                }
                else
                {
                    desc += $"- {"VESWW.RandomThing".Translate(ri.count, ri.quality.ToString(), ri.thingCategories[0].LabelCap)}\n";
                }
            }

            return desc.TrimEndNewlines();
        }
    }
}
