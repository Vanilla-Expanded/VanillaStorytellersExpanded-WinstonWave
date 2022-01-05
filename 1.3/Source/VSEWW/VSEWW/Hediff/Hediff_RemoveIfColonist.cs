using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
