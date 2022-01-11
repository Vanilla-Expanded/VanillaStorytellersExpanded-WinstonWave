using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}

	public class PawnReward
	{
		public PawnKindDef pawnkind;
		public int count;
	}

	public class WaveModifier
    {
		public int delayBy = 0;
		public float weakenBy = 0;
		public bool allies = false;
	}
}
