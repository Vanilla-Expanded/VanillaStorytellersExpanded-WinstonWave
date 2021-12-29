using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VSEWW
{
    internal class NextRaidInfo : IExposable
    {
        public bool sent = false;
        public int atTick;
        public List<string> modifiers = new List<string>();
        public IncidentParms incidentParms;
        public Dictionary<PawnKindDef, int> pawnKinds = new Dictionary<PawnKindDef, int>();
        public int waveNum;
        public int totalPawn;

        public Lord lord;

        public void ExposeData()
        {
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Collections.Look(ref pawnKinds, "pawnKinds");
            Scribe_Deep.Look(ref incidentParms, "incidentParms");
            Scribe_Values.Look(ref waveNum, "waveNum");
            Scribe_Values.Look(ref totalPawn, "totalPawn");
        }

        public string TimeBeforeWave() => TimeSpanExtension.Verbose(TimeSpan.FromSeconds((atTick - Find.TickManager.TicksGame).TicksToSeconds()));

        public List<Pawn> WavePawns()
        {
            if (lord != null)
                return lord.ownedPawns.FindAll(p => p.mindState.Active && p.jobs.AllJobs().Any(j => j.def == JobDefOf.Flee || j.def == JobDefOf.FleeAndCower));
            
            return null;
        }

        public int WavePawnsLeft() => WavePawns().Count;

        public void SetLord()
        {
            Map map = (Map)incidentParms.target;
            if (!sent)
            {
                if (map.lordManager != null && !map.lordManager.lords.NullOrEmpty())
                {
                    lord = map.lordManager.lords.Find(l => l.faction != null && l.faction == incidentParms.faction && !l.ownedPawns.NullOrEmpty());
                    if (lord != null)
                    {
                        sent = true;
                    }
                }
            }
        }
    }
}
