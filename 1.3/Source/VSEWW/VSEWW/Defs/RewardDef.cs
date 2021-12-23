using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VSEWW
{
    public enum RewardCategory
    {
        Poor,
        Normal,
        Good,
        Excellent,
        Legendary
    }

    public abstract class RewardDef : Def
    {
        // defName
        public RewardCategory category;

        public abstract string ToStringHuman();
    }
}
