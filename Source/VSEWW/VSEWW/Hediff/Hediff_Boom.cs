using RimWorld;
using Verse;

namespace VSEWW
{
    public class Hediff_Boom : Hediff_RemoveIfColonist
    {
        public override void Notify_PawnKilled()
        {
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2.9f, DamageDefOf.Bomb, pawn);
            base.Notify_PawnKilled();
        }

        public override void Notify_PawnDied()
        {
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2.9f, DamageDefOf.Bomb, pawn);
            base.Notify_PawnDied();
        }
    }
}