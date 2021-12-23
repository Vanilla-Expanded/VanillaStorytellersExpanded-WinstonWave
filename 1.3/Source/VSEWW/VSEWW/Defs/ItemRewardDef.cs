using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class ItemReward
    {
        public ThingDef thing;
        public QualityCategory quality;
        public int count;
    }

    public class ItemRewardDef : RewardDef
    {
        public List<ItemReward> items;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";

            foreach (ItemReward ir in items.Distinct())
            {
                if (ir.thing.HasComp(typeof(CompQuality)))
                {
                    desc += $"- {"VESWW.ItemWQuality".Translate(ir.count, ir.thing.label, ir.quality.ToString())}\n";
                }
                else
                {
                    desc += $"- {"VESWW.Item".Translate(ir.count, ir.thing.label)}\n";
                }
            }

            return desc.TrimEndNewlines();
        }
    }
}
