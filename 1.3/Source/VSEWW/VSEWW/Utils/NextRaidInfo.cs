using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
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
        public int? modifierCount;
        public bool modifierApplied;
        // - Raid parms
        public IncidentParms incidentParms;
        // - Wave number
        public int waveNum;
        private int? waveType;

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

        public int WaveType
        {
            get
            {
                if (waveType != null)
                    return waveType.Value;

                waveType = waveNum % 5 == 0 ? 1 : 0;

                return waveType.Value;
            }
        }

        public int ModifierCount
        {
            get
            {
                if (modifierCount != null)
                    return modifierCount.Value;

                modifierCount = modifiers.Count;

                return modifierCount.Value;
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
            Scribe_Values.Look(ref modifierApplied, "modifierApplied");
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

        /** Get modifiers chance **/
        private int[] GetModifiersChance()
        {
            int modifierChance = 0;

            if (WaveType == 1) modifierChance += 10;

            if (waveNum > 10)
            {
                if (waveNum <= 15) return new int[] { modifierChance + 3, 0 };
                if (waveNum <= 20) return new int[] { modifierChance + 10, 0 };
                if (waveNum <= 25) return new int[] { modifierChance + 20, 0 };
                if (waveNum <= 30) return new int[] { modifierChance + 25, 0 };
                if (waveNum <= 35) return new int[] { modifierChance + 28, 0 };
                if (waveNum <= 40) return new int[] { modifierChance + 30, 0 };
                if (waveNum <= 45) return new int[] { modifierChance + 35, 0 };
                if (waveNum <= 50) return new int[] { modifierChance + 35, 5 };
                if (waveNum <= 60) return new int[] { modifierChance + 50, 10 };
                return new int[] { modifierChance + 20, 20 };
            }
            return new int[] { modifierChance, 0 };
        }

        /** Choose and add modifier(s) **/
        public void ChooseAndApplyModifier()
        {
            int[] modifiersChance = GetModifiersChance();

            var rand = new Random();
            if (modifiersChance[0] > 0)
            {
                int r = rand.Next(0, 100);
                if (modifiersChance[0] < r)
                    modifiers.Add(DefDatabase<ModifierDef>.AllDefsListForReading.RandomElement());
            }

            if (modifiersChance[1] > 0)
            {
                int r = rand.Next(0, 100);
                if (modifiersChance[1] < r)
                {
                    var chooseFrom = DefDatabase<ModifierDef>.AllDefsListForReading;
                    if (modifiers[0] != null)
                        chooseFrom.Remove(modifiers[0]);
                    modifiers.Add(chooseFrom.RandomElement());
                }
            }

            ApplyModifier();
        }

        /** Apply modifier(s) **/
        public void ApplyModifier()
        {
            foreach (var modifier in modifiers)
            {
                if (!sent && modifier.pointMultiplier > 0) // Can only be applied before raid is sent
                    incidentParms.points *= modifier.pointMultiplier;

                if (sent && Lord != null) // Pawn modifier, only applied if raid is sent and Lord isn't null
                {
                    foreach (var pawn in Lord.ownedPawns)
                    {
                        if (!modifier.globalHediffs.NullOrEmpty())
                        {
                            foreach (var hediff in modifier.globalHediffs)
                            {
                                pawn.health.AddHediff(hediff);
                            }
                        }

                        if (!modifier.techHediffs.NullOrEmpty())
                        {
                            foreach (var hediff in modifier.techHediffs)
                            {
                                InstallPart(pawn, hediff);
                            }
                        }

                        if (!modifier.everRetreat)
                        {
                            pawn.mindState.canFleeIndividual = false;
                        }
                    }
                }
            }
        }

        /** Install part on pawn - copy of vanilla private method **/
        private void InstallPart(Pawn pawn, ThingDef partDef)
        {
            IEnumerable<RecipeDef> source = DefDatabase<RecipeDef>.AllDefs.Where(x => x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x));
            if (!source.Any())
                return;
            RecipeDef recipe = source.RandomElement();
            if (!recipe.Worker.GetPartsToApplyOn(pawn, recipe).Any())
                return;
            recipe.Worker.ApplyOnPawn(pawn, recipe.Worker.GetPartsToApplyOn(pawn, recipe).RandomElement(), null, new List<Thing>(), null);
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
