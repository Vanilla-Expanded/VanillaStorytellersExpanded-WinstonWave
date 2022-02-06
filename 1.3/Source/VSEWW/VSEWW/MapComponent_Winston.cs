using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

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

        internal static readonly List<RaidStrategyDef> normalStrategies = new List<RaidStrategyDef>() { RaidStrategyDefOf.ImmediateAttack, RaidStrategyDefOf.ImmediateAttackFriendly, VDefOf.ImmediateAttackSmart, VDefOf.StageThenAttack };
        // Stat hediff
        private int tickUntilStatCheck = 0;
        private List<Pawn> statPawns = new List<Pawn>();
        private static readonly int checkEachXTicks = 2000;
        private bool once = false;

        public IntVec3 dropSpot = IntVec3.Invalid;
        // counter settings
        internal bool counterDraggable = true;
        internal Vector2 counterPos;

        public MapComponent_Winston(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            if (waveCounter != null)
            {
                counterDraggable = waveCounter.draggable;
                counterPos = new Vector2(waveCounter.windowRect.x + waveCounter.windowRect.width, waveCounter.windowRect.y);
            }

            Scribe_Values.Look(ref currentWave, "currentWave");
            Scribe_Values.Look(ref currentPoints, "currentPoints");
            Scribe_Values.Look(ref modifierChance, "modifierChance");
            Scribe_Values.Look(ref nextRaidSendAllies, "nextRaidSendAllies");
            Scribe_Values.Look(ref nextRaidMultiplyPoints, "nextRaidMultiplyPoints");
            Scribe_Deep.Look(ref nextRaidInfo, "nextRaidInfo");
            Scribe_Values.Look(ref tickUntilStatCheck, "tickUntilStatCheck", 0);
            Scribe_Collections.Look(ref statPawns, "statPawns", LookMode.Reference);
            Scribe_Values.Look(ref counterDraggable, "counterDraggable");
            Scribe_Values.Look(ref counterPos, "counterPos");
            Scribe_Values.Look(ref once, "once");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (counterPos.x == 0 && counterPos.y == 0)
            {
                counterPos = new Vector2(UI.screenWidth - 5f, 5f);
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (map.ParentFaction == Faction.OfPlayer)
            {
                if (Find.Storyteller.def.defName == "VSE_WinstonWave" && Find.Storyteller.difficultyDef != DifficultyDefOf.Peaceful)
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
                        float inD = currentWave > 1 ? VESWWMod.settings.timeBetweenWaves : VESWWMod.settings.timeBeforeFirstWave;
                        nextRaidInfo = currentWave % 5 == 0 ? SetNextBossRaidInfo(inD) : SetNextNormalRaidInfo(inD);
                    }
                    else
                    {
                        if (!nextRaidInfo.sent && nextRaidInfo.atTick <= Find.TickManager.TicksGame)
                        {
                            ExecuteRaid(Find.TickManager.TicksGame);
                        }
                        else if (nextRaidInfo.sent && nextRaidInfo.Lords != null && nextRaidInfo.WavePawnsLeft() == 0 && map.mapPawns.AnyColonistSpawned)
                        {
                            Find.WindowStack.Add(new Window_ChooseReward(currentWave, nextRaidInfo.FourthRewardChance));
                        }
                        else if (nextRaidInfo.sent && nextRaidInfo.Lords == null && Find.TickManager.TicksGame - nextRaidInfo.atTick > 1000)
                        {
                            var at = Find.TickManager.TicksGame + 500;
                            nextRaidInfo = currentWave % 5 == 0 ? SetNextBossRaidInfo(at) : SetNextNormalRaidInfo(at);
                        }
                    }

                    if (waveCounter == null)
                    {
                        waveCounter = new Window_WaveCounter(this, counterDraggable, counterPos);
                        Find.WindowStack.Add(waveCounter);
                        waveCounter.UpdateHeight();
                        waveCounter.UpdateWidth();
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
                        counterPos = new Vector2(waveCounter.windowRect.x + waveCounter.windowRect.width, waveCounter.windowRect.y);
                        counterDraggable = waveCounter.draggable;
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
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                incidentParms.target = map;
                incidentParms.faction = Find.FactionManager.RandomAlliedFaction();
                incidentParms.points = Math.Min(nextRaidInfo.incidentParms.points * 2, VESWWMod.settings.maxPoints);

                IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
                nextRaidSendAllies = false;
            }
            nextRaidInfo.sent = true;
        }

        internal NextRaidInfo SetNextNormalRaidInfo(float inDays)
        {
            NextRaidInfo nri = new NextRaidInfo()
            {
                incidentParms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                    faction = FindRandomEnnemy(),
                    pawnGroupMakerSeed = new System.Random().Next(1, 10000)
                },
                atTick = Find.TickManager.TicksGame + (int)(inDays * 60000),
                generatedAt = Find.TickManager.TicksGame,
                waveNum = currentWave
            };
            nri.incidentParms.raidStrategy = normalStrategies.Find(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat));

            nri.ChooseAndApplyModifier();
            nri.SetPawnsInfo();
            waveCounter?.UpdateHeight();
            return nri;
        }

        internal NextRaidInfo SetNextBossRaidInfo(float inDays)
        {
            NextRaidInfo nri = new NextRaidInfo()
            {
                incidentParms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    faction = FindRandomEnnemy(),
                    pawnGroupMakerSeed = new System.Random().Next(1, 10000)
                },
                atTick = Find.TickManager.TicksGame + (int)(inDays * 60000),
                generatedAt = Find.TickManager.TicksGame,
                waveNum = currentWave
            };
            var list = DefDatabase<RaidStrategyDef>.AllDefsListForReading.FindAll(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat)
                                                                                       && !normalStrategies.Contains(s)
                                                                                       && (VESWWMod.settings.excludedStrategyDefs.NullOrEmpty()
                                                                                           || !VESWWMod.settings.excludedStrategyDefs.Contains(s.defName)));
            nri.incidentParms.raidStrategy = list.NullOrEmpty() ? normalStrategies.Find(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat)) : list.RandomElement();

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

                // currentPoints *= Find.Storyteller.difficulty.threatScale;
            }

            if (currentPoints < 100)
                currentPoints = 100;

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
            statPawns?.ForEach(p =>
            {
                var hediff = p.health.hediffSet.GetFirstHediffOfDef(VDefOf.VESWW_IncreasedStats);
                if (hediff != null)
                    p.health.RemoveHediff(hediff);
            });
            statPawns = new List<Pawn>();
        }

        internal Faction FindRandomEnnemy()
        {
            var from = Find.FactionManager.AllFactions.ToList().FindAll(f =>
                            (VESWWMod.settings.excludedFactionDefs == null || !VESWWMod.settings.excludedFactionDefs.Contains(f.def.defName))
                            && f.HostileTo(Faction.OfPlayer)
                            && !f.def.pawnGroupMakers.NullOrEmpty()
                            && currentPoints > f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));

            if (from.NullOrEmpty())
            {
                Find.Storyteller.difficultyDef = DifficultyDefOf.Peaceful;
                Log.Error($"[VSEWW] No ennemy faction has been found. Switching to Peaceful to prevent further errors.");
                return null;
            }

            return from.RandomElement();
        }
    }
}
