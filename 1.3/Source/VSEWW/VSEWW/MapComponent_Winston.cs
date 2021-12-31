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
    internal class MapComponent_Winston : MapComponent
    {
        internal int currentWave = 1;
        internal float currentPoints = 0;
        internal float modifierChance = 0;

        internal bool nextRaidSendAllies = false;
        internal float nextRaidMultiplyPoints = 1f;
        internal NextRaidInfo nextRaidInfo;

        internal Window_WaveCounter waveCounter = null;


        public MapComponent_Winston(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentWave, "currentWave");
            Scribe_Values.Look(ref currentPoints, "currentPoints");
            Scribe_Values.Look(ref modifierChance, "modifierChance");
            Scribe_Values.Look(ref nextRaidSendAllies, "nextRaidSendAllies");
            Scribe_Values.Look(ref nextRaidMultiplyPoints, "nextRaidMultiplyPoints");
            Scribe_Deep.Look(ref nextRaidInfo, "nextRaidInfo");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                if (nextRaidInfo == null || nextRaidInfo.incidentParms.raidStrategy == null || nextRaidInfo.incidentParms.faction == null)
                {
                    int inD = currentWave > 1 ? VESWWMod.settings.timeBetweenWaves : VESWWMod.settings.timeBeforeFirstWave;
                    nextRaidInfo = currentWave % 5 == 0 ? SetNextBossRaidInfo(inD) : SetNextNormalRaidInfo(inD);
                }
                else
                {
                    nextRaidInfo.SetLord();
                    if (!nextRaidInfo.sent && nextRaidInfo.atTick <= Find.TickManager.TicksGame)
                    {
                        ExecuteRaid(Find.TickManager.TicksGame);
                    }
                    else if (nextRaidInfo.sent && nextRaidInfo.lord != null && nextRaidInfo.WavePawnsLeft() == 0)
                    {
                        Find.WindowStack.Add(new Window_ChooseReward(currentWave));
                        if (++currentWave % 5 == 0)
                        {
                            nextRaidInfo = SetNextBossRaidInfo(VESWWMod.settings.timeBetweenWaves);
                        }
                        else
                        {
                            nextRaidInfo = SetNextNormalRaidInfo(VESWWMod.settings.timeBetweenWaves);
                        }
                        waveCounter.UpdateHeightAndWidth();
                    }
                }

                if (waveCounter == null)
                {
                    waveCounter = new Window_WaveCounter(this);
                    Find.WindowStack.Add(waveCounter);
                    waveCounter.UpdateHeightAndWidth();
                }
            }
            else if (nextRaidInfo != null)
            {
                nextRaidInfo.atTick++;
            }
        }

        private void ExecuteRaid(int tick)
        {
            ++Find.StoryWatcher.statsRecord.numRaidsEnemy;
            map.StoryState.lastRaidFaction = nextRaidInfo.incidentParms.faction;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, tick, nextRaidInfo.incidentParms);

            if (nextRaidSendAllies)
            {
                IncidentParms incidentParms = new IncidentParms()
                {
                    target = map,
                    faction = Find.FactionManager.RandomAlliedFaction(),
                };
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, tick, incidentParms);
                nextRaidSendAllies = false;
            }
        }

        internal NextRaidInfo SetNextNormalRaidInfo(int inDays)
        {
            List<RaidStrategyDef> from = new List<RaidStrategyDef>() { RaidStrategyDefOf.ImmediateAttack, DefDatabase<RaidStrategyDef>.GetNamed("ImmediateAttackSmart"), DefDatabase<RaidStrategyDef>.GetNamed("StageThenAttack")};
            NextRaidInfo nri = new NextRaidInfo()
            {
                incidentParms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                    faction = Find.FactionManager.RandomEnemyFaction(false, false),
                    pawnGroupMakerSeed = new Random().Next(1, 10000)
                },
                atTick = Find.TickManager.TicksGame + (inDays * 60000),
                waveNum = currentWave
            };
            nri.incidentParms.raidStrategy = from.Find(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat));

            var pList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, nri.incidentParms)).ToList();
            // Get all kinds and the number of them
            foreach (Pawn pawn in pList)
            {
                if (nri.pawnKinds.ContainsKey(pawn.kindDef))
                {
                    nri.pawnKinds[pawn.kindDef]++;
                }
                else
                {
                    nri.pawnKinds[pawn.kindDef] = 1;
                }
            }
            nri.totalPawn = nri.pawnKinds.Sum(k => k.Value);

            string kindLabel = "VESWW.EnemiesC".Translate() + "\n";
            nri.kindListLines++;
            foreach (var pair in nri.pawnKinds)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                nri.kindListLines++;
            }
            nri.kindList = kindLabel.TrimEndNewlines();

            ChooseAndApplyModifier(nri);
            return nri;
        }

        internal NextRaidInfo SetNextBossRaidInfo(int inDays)
        {
            NextRaidInfo nri = new NextRaidInfo()
            {
                incidentParms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    faction = Find.FactionManager.RandomEnemyFaction(false, false),
                    pawnGroupMakerSeed = new Random().Next(1, 10000)
                },
                atTick = Find.TickManager.TicksGame + (inDays * 60000),
                waveNum = currentWave
            };
            nri.incidentParms.raidStrategy = DefDatabase<RaidStrategyDef>.AllDefsListForReading.FindAll(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat) &&
                                                    s != RaidStrategyDefOf.ImmediateAttack && s != DefDatabase<RaidStrategyDef>.GetNamed("ImmediateAttackSmart") && 
                                                    s != DefDatabase<RaidStrategyDef>.GetNamed("StageThenAttack"))
                .RandomElement();

            var pList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, nri.incidentParms)).ToList();
            // Get all kinds and the number of them
            foreach (Pawn pawn in pList)
            {
                if (nri.pawnKinds.ContainsKey(pawn.kindDef))
                {
                    nri.pawnKinds[pawn.kindDef]++;
                }
                else
                {
                    nri.pawnKinds[pawn.kindDef] = 1;
                }
            }
            nri.totalPawn = nri.pawnKinds.Sum(k => k.Value);

            string kindLabel = "VESWW.EnemiesC".Translate() + "\n";
            nri.kindListLines++;
            foreach (var pair in nri.pawnKinds)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                nri.kindListLines++;
            }
            nri.kindList = kindLabel.TrimEndNewlines();

            ChooseAndApplyModifier(nri);
            return nri;
        }

        internal float GetNextWavePoint()
        {
            if (currentPoints < VESWWMod.settings.maxPoints)
            {
                if (currentPoints <= 0) currentPoints = 100f;
                else currentPoints *= 1.2f;
            }

            float point = currentPoints * nextRaidMultiplyPoints;
            nextRaidMultiplyPoints = 1f;

            return point;
        }
    
        private void ChooseAndApplyModifier(NextRaidInfo nri)
        {
            // TODO Modifers
        }
    }
}
