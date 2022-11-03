using RimWorld;

namespace VSEWW
{
    [DefOf]
    public static class WRaidStrategyDefOf
    {
        public static RaidStrategyDef ImmediateAttackSmart;
        public static RaidStrategyDef StageThenAttack;

        static WRaidStrategyDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(WRaidStrategyDefOf));
    }
}
