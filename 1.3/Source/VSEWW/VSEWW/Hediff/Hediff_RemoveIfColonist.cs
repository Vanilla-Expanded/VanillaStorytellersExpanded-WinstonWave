using RimWorld;
using Verse;

namespace VSEWW
{
    public class Hediff_RemoveIfColonist : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn.Faction == Faction.OfPlayer)
                pawn.health.RemoveHediff(this);
        }
    }
}
