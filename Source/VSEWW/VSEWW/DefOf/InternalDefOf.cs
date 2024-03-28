using RimWorld;

namespace VSEWW
{
    [DefOf]
    public static class InternalDefOf
    {
        public static DifficultyDef Peaceful;

        static InternalDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
    }
}
