using RimWorld;
using Verse;

namespace VSEWW
{
    [DefOf]
    public static class InternalDefOf
    {
        public static DifficultyDef Peaceful;
        [MayRequireAnomaly]
        public static ThingDef CreepJoiner;

        static InternalDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
    }
}
