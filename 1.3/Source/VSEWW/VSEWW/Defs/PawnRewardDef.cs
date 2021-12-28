using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class PawnReward
    {
        public PawnKindDef pawnkind;
        public int count;
    }

    public class PawnRewardDef : RewardDef
    {
        public List<PawnReward> pawns;

        /*public override string ToStringHuman()
        {
            string desc = "";

            foreach (PawnReward pr in pawns.Distinct())
            {
                desc += $"- {"VESWW.Pawn".Translate(pr.count, (pr.count > 1 ? pr.pawnkind.labelPlural : pr.pawnkind.label))}\n";
            }

            return desc.TrimEndNewlines();
        }*/
    }
}
