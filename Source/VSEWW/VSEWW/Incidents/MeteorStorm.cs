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
        public static List<ThingDef> nonSmoothedMineables = new List<ThingDef>();
        public static readonly IntRange MineablesCountRange = new IntRange(2, 8);

        public static void Reset()
        {
            nonSmoothedMineables.Clear();
            nonSmoothedMineables.Add(ThingDefOf.Granite);
        }

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            int randomInRange = (parms.countRange ?? MineablesCountRange).RandomInRange;
            ThingDef randomMineableDef = ThingDefOf.Granite;
            for (int index = 0; index < randomInRange; ++index)
            {
                Building building = (Building)ThingMaker.MakeThing(randomMineableDef);
                building.canChangeTerrainOnDestroyed = false;
                outThings.Add(building);
            }
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms) => nonSmoothedMineables;
    }

    public class MeteorStorm : GameCondition
    {
        private IntVec3 nextMeteorCell = new IntVec3();
        private readonly int meteorIntervalTicks = 150;
        private int ticksToNextEffect;

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_StoneMeteorite.MineablesCountRange.max;
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
            if (!nextMeteorCell.IsValid)
            {
                ticksToNextEffect = meteorIntervalTicks;
                TryFindCell(out nextMeteorCell, map);
            }
            ticksToNextEffect--;
            if (ticksToNextEffect <= 0 && TicksLeft >= meteorIntervalTicks)
            {
                var list = ThingSetMakerMeteorite.VESWW_Meteorite.root.Generate();
                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, nextMeteorCell, map);
                ticksToNextEffect = meteorIntervalTicks;
                TryFindCell(out nextMeteorCell, map);
            }
        }
    }
}