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

        private static readonly List<RaidStrategyDef> normalStrategies = new List<RaidStrategyDef>() { RaidStrategyDefOf.ImmediateAttack, VDefOf.ImmediateAttackSmart, VDefOf.StageThenAttack };
        // Stat hediff
        private int tickUntilStatCheck = 0;
        private List<Pawn> statPawns = new List<Pawn>();
        private static readonly int checkEachXTicks = 2000;
        private bool once = false;

        public IntVec3 dropSpot = IntVec3.Invalid;
          
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
            Scribe_Values.Look(ref tickUntilStatCheck, "tickUntilStatCheck", 0);
            Scribe_Collections.Look(ref statPawns, "statPawns", LookMode.Reference);
            Scribe_Values.Look(ref once, "once");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (map.ParentFaction == Faction.OfPlayer)
            {
                if (Find.Storyteller.def.defName == "VSE_WinstonWave")
                {
                    if (VESWWMod.settings.enableStatIncrease)
                    {
                        if (tickUntilStatCheck <= 0)
                        {
                            AddStatHediff();
                            tickUntilStatCheck = checkEachXTicks;
                            once = false;
                        }
                        tickUntilStatCheck--;
                    }

                    if (nextRaidInfo == null || nextRaidInfo.incidentParms.raidStrategy == null || nextRaidInfo.incidentParms.faction == null)
                    {
                        int inD = currentWave > 1 ? VESWWMod.settings.timeBetweenWaves : VESWWMod.settings.timeBeforeFirstWave;
                        nextRaidInfo = currentWave % 5 == 0 ? SetNextBossRaidInfo(inD) : SetNextNormalRaidInfo(inD);
                    }
                    else
                    {
                        if (!nextRaidInfo.sent && nextRaidInfo.atTick <= Find.TickManager.TicksGame)
                        {
                            ExecuteRaid(Find.TickManager.TicksGame);
                        }
                        else if (nextRaidInfo.sent && nextRaidInfo.Lords != null && nextRaidInfo.WavePawnsLeft() == 0)
                        {
                            Find.WindowStack.Add(new Window_ChooseReward(currentWave, nextRaidInfo.FourthRewardChance));
                            nextRaidInfo.StopEvents();
                            if (++currentWave % 5 == 0)
                            {
                                nextRaidInfo = SetNextBossRaidInfo(VESWWMod.settings.timeBetweenWaves);
                            }
                            else
                            {
                                nextRaidInfo = SetNextNormalRaidInfo(VESWWMod.settings.timeBetweenWaves);
                            }
                            waveCounter.UpdateHeight();
                            waveCounter.WaveTip();
                        }
                    }

                    if (waveCounter == null)
                    {
                        waveCounter = new Window_WaveCounter(this);
                        Find.WindowStack.Add(waveCounter);
                        waveCounter.UpdateHeight();
                    }
                }
                else
                {
                    if (nextRaidInfo != null)
                        nextRaidInfo.atTick++;

                    if (!once && VESWWMod.settings.enableStatIncrease)
                    {
                        RemoveStatHediff();
                        once = true;
                        tickUntilStatCheck = 0; // Instant stat back if switch storyteller
                    }

                    if (waveCounter != null)
                    {
                        waveCounter.Close();
                        waveCounter = null;
                    }
                }
            }
        }

        internal void ExecuteRaid(int tick)
        {
            ++Find.StoryWatcher.statsRecord.numRaidsEnemy;
            map.StoryState.lastRaidFaction = nextRaidInfo.incidentParms.faction;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, tick, nextRaidInfo.incidentParms);
            nextRaidInfo.SendAddditionalModifier();
            nextRaidInfo.sentAt = Find.TickManager.TicksGame;
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
            nextRaidInfo.sent = true;
        }

        internal NextRaidInfo SetNextNormalRaidInfo(int inDays)
        {
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
            nri.incidentParms.raidStrategy = normalStrategies.Find(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat));

            nri.ChooseAndApplyModifier();
            nri.SetPawnsInfo();
            waveCounter?.UpdateHeight();
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
                                                    !normalStrategies.Contains(s))
                .RandomElement();

            nri.ChooseAndApplyModifier();
            nri.SetPawnsInfo();
            waveCounter?.UpdateHeight();
            return nri;
        }

        internal float GetNextWavePoint()
        {
            if (currentPoints < VESWWMod.settings.maxPoints)
            {
                if (currentPoints <= 0) currentPoints = 100f;
                else if (currentWave <= 20) currentPoints *= VESWWMod.settings.pointMultiplierBefore;
                else currentPoints *= VESWWMod.settings.pointMultiplierAfter;
            }

            float point = currentPoints * nextRaidMultiplyPoints;
            nextRaidMultiplyPoints = 1f;

            return Math.Min(point, VESWWMod.settings.maxPoints);
        }

        internal void RegisterDropSpot(CompRegisterAsRewardDrop comp) => dropSpot = comp.parent.Position;
        
        internal void UnRegisterDropSpot() => dropSpot = IntVec3.Invalid;
    
        internal void AddStatHediff()
        {
            map.mapPawns.AllPawnsSpawned.FindAll(p => p.Faction == Faction.OfPlayer && p.RaceProps.Humanlike).ForEach(p =>
            {
                if (!statPawns.Contains(p) && p.health != null)
                {
                    p.health.AddHediff(VDefOf.VESWW_IncreasedStats);
                    statPawns.Add(p);
                }
            });
        }
        
        internal void RemoveStatHediff()
        {
            statPawns.ForEach(p =>
            {
                var hediff = p.health.hediffSet.GetFirstHediffOfDef(VDefOf.VESWW_IncreasedStats);
                if (hediff != null)
                    p.health.RemoveHediff(hediff);
            });
            statPawns = new List<Pawn>();
        }
    }
}
