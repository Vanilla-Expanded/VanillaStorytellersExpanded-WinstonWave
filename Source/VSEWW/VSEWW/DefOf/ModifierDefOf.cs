using RimWorld;

namespace VSEWW
{
    [DefOf]
    public static class ModifierDefOf
    {
        public static ModifierDef VSEWW_Mystery;

        static ModifierDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ModifierDefOf));
    }
}
