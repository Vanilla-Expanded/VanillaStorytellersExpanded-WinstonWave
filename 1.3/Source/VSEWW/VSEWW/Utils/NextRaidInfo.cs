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
        // Raid infos
        // - Is always false at start - Set to true when lord isn't null
        public bool sent = false;
        private Lord lord;
        // - When
        public int atTick;
        // - All modifiers applied to the raid
        public List<ModifierDef> modifiers = new List<ModifierDef>();
        // - Raid parms
        public IncidentParms incidentParms;
        // - Wave number
        public int waveNum;

        // Utils for wave counter
        // - Wave prediction string && size
        public string kindList;
        public int kindListLines;
        // - Wave progress cached alive pawns
        private int cacheTick = 0;
        private List<Pawn> lordPawnsCache = new List<Pawn>();
        internal string cacheKindList;
        // - Number of pawns at the start
        public int totalPawn;

        public Lord Lord
        {
            get
            {
                if (lord != null)
                    return lord;

                Map map = (Map)incidentParms.target;
                if (map.lordManager != null && !map.lordManager.lords.NullOrEmpty())
                {
                    lord = map.lordManager.lords.Find(l => l.faction != null && l.faction == incidentParms.faction && !l.ownedPawns.NullOrEmpty());
                }

                return lord ?? null;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref sent, "sent");
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Deep.Look(ref incidentParms, "incidentParms");
            Scribe_Values.Look(ref waveNum, "waveNum");
            Scribe_Values.Look(ref totalPawn, "totalPawn");
        }

        /** Get IRL time before this raid **/
        public string TimeBeforeWave() => TimeSpanExtension.Verbose(TimeSpan.FromSeconds((atTick - Find.TickManager.TicksGame).TicksToSeconds()));

        /** Get all pawns part of the raid - with caching **/
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

        /** Get pawns count left **/
        public int WavePawnsLeft() => WavePawns().Count;

        /** Choose and add modifier(s) **/
        public void ChooseAndApplyModifier(float modifierChance)
        {

        }

        /** Set pawns prediction string and count **/
        public void SetPawnsInfo()
        {
            var pList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms)).ToList();
            // Get all kinds and the number of them
            var tempDic = new Dictionary<PawnKindDef, int>();
            foreach (Pawn pawn in pList)
            {
                if (tempDic.ContainsKey(pawn.kindDef))
                {
                    tempDic[pawn.kindDef]++;
                }
                else
                {
                    tempDic[pawn.kindDef] = 1;
                }
            }
            totalPawn = tempDic.Sum(k => k.Value);

            string kindLabel = "VESWW.EnemiesC".Translate() + "\n";
            kindListLines++;
            foreach (var pair in tempDic)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                kindListLines++;
            }
            kindList = kindLabel.TrimEndNewlines();
        }
    }
}
