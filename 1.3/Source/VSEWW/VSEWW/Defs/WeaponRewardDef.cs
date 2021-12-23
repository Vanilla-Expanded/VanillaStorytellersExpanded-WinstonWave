using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class WeaponReward
    {
        public string weaponTag;
        public TechLevel maxTechLevel;
        public QualityCategory quality;
        public int count;
    }

    public class WeaponRewardDef : RewardDef
    {
        public List<WeaponReward> weapons;

        public override string ToStringHuman()
        {
            string desc = "VESWW.RewardContain".Translate(this.category.ToString()) + "\n";

            foreach (WeaponReward wp in weapons.Distinct())
            {
                desc += $"- {"VESWW.RandomWeapons".Translate(wp.count, wp.quality.ToString(), wp.weaponTag)}\n";
            }

            return desc.TrimEndNewlines();
        }
    }
}
