using RimWorld;
using Verse;

namespace VSEWW
{
    [DefOf]
    public static class WHediffDefOf
    {
        public static HediffDef VSEWW_BulletSponge;
        public static HediffDef VESWW_IncreasedStats;

        static WHediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(WHediffDefOf));
    }
}
