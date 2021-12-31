using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
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
        public string kindList;
        public int kindListLines;
        public int waveNum;
        public int totalPawn;

        public Lord lord;
        private List<Pawn> lordPawnsCache = new List<Pawn>();
        private int cacheTick = 0;
        internal string cacheKindList;

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
            if (cacheTick % 800 == 0 || lordPawnsCache.NullOrEmpty())
            {
                string kindLabel = "VESWW.EnemiesR".Translate() + "\n";
                Dictionary<PawnKindDef, int> toDefeat = new Dictionary<PawnKindDef, int>();
                lordPawnsCache = lord.ownedPawns.FindAll(p => !p.Downed && !p.mindState.mentalStateHandler.InMentalState);
                lordPawnsCache.ForEach(p =>
                {
                    if (toDefeat.ContainsKey(p.kindDef))
                        toDefeat[p.kindDef]++;
                    else
                        toDefeat.Add(p.kindDef, 1);
                });
                foreach (var pair in toDefeat)
                {
                    kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                }
                cacheKindList = kindLabel.TrimEndNewlines();
            }

            cacheTick++;
            return lordPawnsCache;
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
