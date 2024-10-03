using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VSEWW
{
    public class RPawnReward
    {
        public string tradeTag = "";
        public Intelligence intelligence = Intelligence.Humanlike;
        public int maxCombatPower;
        public int minCombatPower;
        public int count;
        public bool excludeInsectoid = false;
        public bool ghoul = false;
        public bool shambler = false;
        public List<XenotypeDef> randomXenotypeFrom;
        
    }

    public class ItemReward
    {
        public ThingDef thing;
        public QualityCategory quality;
        public int count;
    }

    public class RItemReward
    {
        public List<ThingCategoryDef> thingCategories;
        public List<ThingCategoryDef> excludeThingCategories;
        public QualityCategory quality;
        public int count;
        public string tradeTag;
        public List<ThingDef> randomFrom;
    }

    public class PawnReward
    {
        public PawnKindDef pawnkind;
        public int count;
        public List<XenotypeDef> randomXenotypeFrom;
    }

    public class WaveModifier
    {
        public int delayBy = 0;
        public float weakenBy = 0;
        public bool allies = false;
    }
}