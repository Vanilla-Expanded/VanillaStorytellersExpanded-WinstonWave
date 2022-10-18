using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    [DefOf]
    public static class ThingSetMakerMeteorite
    {
        public static ThingSetMakerDef VESWW_Meteorite;

        static ThingSetMakerMeteorite() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDefOf));
    }

    public class ThingSetMaker_StoneMeteorite : ThingSetMaker
    {
        public static IntRange mineablesCountRange = new IntRange(8, 16);

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            for (int index = 0; index < mineablesCountRange.RandomInRange; ++index)
            {
                Building building = (Building)ThingMaker.MakeThing(ThingDefOf.Granite);
                building.canChangeTerrainOnDestroyed = false;
                outThings.Add(building);
            }
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms) => new List<ThingDef> { ThingDefOf.Granite };
    }

    public class MeteorStorm : GameCondition
    {
        const int meteorIntervalTicks = 350;

        private IntVec3 nextMeteorCell = new IntVec3();
        private int ticksToNextEffect;

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_StoneMeteorite.mineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default, -1, true, false, false, false, true, true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
                CellRect cellRect = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                foreach (IntVec3 c in cellRect)
                {
                    if (c.InBounds(map) && c.Standable(map))
                    {
                        num2++;
                    }
                }
                return num2 >= maxMineables;
            });
        }

        public override void GameConditionTick()
        {
            Map map = SingleMap;

            // Explosion handle
            ticksToNextEffect--;
            if (ticksToNextEffect <= 0 && TicksLeft >= meteorIntervalTicks && TryFindCell(out nextMeteorCell, map))
            {
                var list = ThingSetMakerMeteorite.VESWW_Meteorite.root.Generate();
                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, nextMeteorCell, map);
                ticksToNextEffect = meteorIntervalTicks;
            }
        }
    }
}