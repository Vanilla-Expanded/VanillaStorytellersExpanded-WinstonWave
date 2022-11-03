using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class MapComponent_Winston : MapComponent
    {
        internal static readonly List<RaidStrategyDef> normalStrategies = new List<RaidStrategyDef>() { RaidStrategyDefOf.ImmediateAttack, RaidStrategyDefOf.ImmediateAttackFriendly, WDefOf.ImmediateAttackSmart, WDefOf.StageThenAttack };
        internal List<RaidStrategyDef> allOtherStrategies;

        internal Vector2 counterPos;
        internal bool counterDraggable = true;

        internal int currentWave = 1;
        internal float currentPoints = 0;
        internal float modifierChance = 0;

        internal NextRaidInfo nextRaidInfo;
        internal float nextRaidMultiplyPoints = 1f;
        internal bool nextRaidSendAllies = false;

        internal Window_WaveCounter waveCounter = null;

        internal IntVec3 dropSpot = IntVec3.Invalid;

        internal bool sosSpace;

        internal int tickUntilStatCheck = 0;
        internal List<Pawn> statPawns = new List<Pawn>();
        internal static readonly int checkEachXTicks = 2000;

        internal Faction mapFaction;
        internal MapParent mapParent;

        internal bool ShouldRegenerateRaid => nextRaidInfo == null || nextRaidInfo.parms.raidStrategy == null || nextRaidInfo.parms.faction == null || nextRaidInfo.totalPawnsBefore == 0;

        public MapComponent_Winston(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            if (waveCounter != null)
            {
                counterDraggable = waveCounter.draggable;
                counterPos = new Vector2(UI.screenWidth - waveCounter.windowRect.xMax, waveCounter.windowRect.y);
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
        }

        public override void FinalizeInit()
        {
            if (counterPos.x == 0 && counterPos.y == 0)
                counterPos = new Vector2(5f, 5f);

            allOtherStrategies = DefDatabase<RaidStrategyDef>.AllDefsListForReading;
            allOtherStrategies.RemoveAll(s => normalStrategies.Contains(s));

            sosSpace = map.Biome.defName == "OuterSpaceBiome";
            mapFaction = map.ParentFaction;
            mapParent = map.Parent;
        }

        public override void MapComponentTick()
        {
            // No waves in space, other faction maps, if there is a window
            if (sosSpace || (mapParent != null && !mapParent.def.canBePlayerHome) || mapFaction != Faction.OfPlayer || Find.WindowStack.AnyWindowAbsorbingAllInput)
                return;

            var storyteller = Find.Storyteller;
            var ticksGame = Find.TickManager.TicksGame;
            // If winston selected and not peaceful
            if (storyteller.def.defName == "VSE_WinstonWave" && storyteller.difficultyDef != DifficultyDefOf.Peaceful)
            {
                // If stats increase enabled
                if (WinstonMod.settings.enableStatIncrease)
                {
                    // Check
                    if (tickUntilStatCheck <= 0)
                    {
                        // Add
                        AddStatHediff();
                        tickUntilStatCheck = checkEachXTicks;
                    }
                    tickUntilStatCheck--;
                }
                // If next raid isn't set, or is bugged
                if (ShouldRegenerateRaid)
                {
                    nextRaidInfo = GetNextRaid(ticksGame);
                    waveCounter?.UpdateHeight();
                    waveCounter?.WaveTip();
                }
                else
                {
                    // If raid isn't sent, but should be
                    if (!nextRaidInfo.sent && nextRaidInfo.atTick <= ticksGame)
                    {
                        StartRaid(ticksGame);
                    }
                    // If raid is over
                    else if (nextRaidInfo.RaidOver)
                    {
                        PrepareNextWave(ticksGame);
                    }
                    // If player took too long to finish the wave
                    else if (nextRaidInfo.sent && ticksGame - nextRaidInfo.sentAt >= WinstonMod.settings.timeToDefeatWave * 60000)
                    {
                        PrepareNextWave(ticksGame, false);
                    }
                    // If raid lords fail
                    else if (nextRaidInfo.sent && nextRaidInfo.Lords == null && ticksGame - nextRaidInfo.atTick > 1000)
                    {
                        // Regenerate raid
                        nextRaidInfo = GetNextRaid(ticksGame);
                    }
                }
                // Manage counter visibility
                var currentMap = Find.CurrentMap;
                if (waveCounter == null && currentMap == map)
                {
                    waveCounter = new Window_WaveCounter(this, counterDraggable, counterPos);
                    Find.WindowStack.Add(waveCounter);
                    waveCounter.UpdateHeight();
                    waveCounter.WaveTip();
                }
                else if (waveCounter != null && currentMap != map)
                {
                    RemoveCounter();
                }
            }
            // If winston ins't selected or it's peaceful
            else
            {
                // Delay next raid
                if (nextRaidInfo != null)
                    nextRaidInfo.atTick++;
                // Remove all stats increase hediffs
                if (statPawns.NullOrEmpty())
                {
                    RemoveStatHediff();
                    tickUntilStatCheck = 0; // Instant stat back if switch storyteller
                }
                // Remove the counter
                if (waveCounter != null)
                    RemoveCounter();
            }
        }

        /// <summary>
        /// Call functions and prepare the next wave and send reward if wanted
        /// </summary>
        internal void PrepareNextWave(int ticksGame, bool sendReward = true)
        {
            // Prepare next wave
            currentWave++;
            nextRaidInfo.StopIncidentModifiers();
            nextRaidInfo = GetNextRaid(ticksGame);
            waveCounter?.UpdateHeight();
            waveCounter?.WaveTip();
            // Show rewards window
            if (sendReward)
                Find.WindowStack.Add(new Window_ChooseReward(currentWave, nextRaidInfo.FourthRewardChance, map));
        }

        /// <summary>
        /// Generate either a normal or a boss wave depending on the current wave number
        /// </summary>
        internal NextRaidInfo GetNextRaid(int ticks)
        {
            float days = currentWave > 1 ? WinstonMod.settings.timeBetweenWaves : WinstonMod.settings.timeBeforeFirstWave;
            return currentWave % 5 == 0 ? SetNextBossRaidInfo(ticks, days) : SetNextNormalRaidInfo(ticks, days);
        }

        /// <summary>
        /// Calculate new wave raid points
        /// </summary>
        internal float GetNextWavePoint()
        {
            if (currentPoints < WinstonMod.settings.maxPoints)
            {
                if (currentPoints <= 0)
                    currentPoints = 100f;
                else if (currentWave <= 20)
                    currentPoints *= WinstonMod.settings.pointMultiplierBefore;
                else
                    currentPoints *= WinstonMod.settings.pointMultiplierAfter;
            }

            if (currentPoints < 100)
                currentPoints = 100;

            float point = currentPoints * nextRaidMultiplyPoints;
            nextRaidMultiplyPoints = 1f;

            return Mathf.Min(point, WinstonMod.settings.maxPoints);
        }

        /// <summary>
        /// Find random faction for the next wave
        /// </summary>
        internal Faction RandomEnnemyFaction()
        {
            var from = Find.FactionManager.AllFactions.ToList().FindAll(f =>
                            (WinstonMod.settings.excludedFactionDefs == null || !WinstonMod.settings.excludedFactionDefs.Contains(f.def.defName))
                            && !f.temporary
                            && !f.defeated
                            && f.HostileTo(Faction.OfPlayer)
                            && f.def.pawnGroupMakers != null
                            && f.def.pawnGroupMakers.Any(p => p.kindDef == PawnGroupKindDefOf.Combat && currentPoints <= p.maxTotalPoints)
                            && currentPoints > f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));

            if (from.NullOrEmpty())
            {
                Find.Storyteller.difficultyDef = DifficultyDefOf.Peaceful;
                Log.Error($"[VSEWW] No ennemy faction has been found. Switching to Peaceful to prevent further errors.");
                return null;
            }

            from.TryRandomElementByWeight(f =>
            {
                float num = 1f;
                if (map.StoryState != null && map.StoryState.lastRaidFaction != null && f == map.StoryState.lastRaidFaction)
                {
                    num = 0.4f;
                }
                return f.def.RaidCommonalityFromPoints(currentPoints) * num;
            }, out Faction faction);

            return faction;
        }

        /// <summary>
        /// Create next boss raid
        /// </summary>
        internal NextRaidInfo SetNextBossRaidInfo(int ticks, float inDays)
        {
            NextRaidInfo nri = new NextRaidInfo()
            {
                parms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    faction = RandomEnnemyFaction()
                },
                atTick = ticks + (int)(inDays * 60000),
                generatedAt = ticks,
                waveNum = currentWave,
                waveType = currentWave % 5 == 0 ? 1 : 0,
                map = map
            };

            var list = allOtherStrategies.FindAll(s => CanUseStrategy(s, nri));
            if (list.NullOrEmpty())
            {
                normalStrategies.FindAll(s => CanUseStrategy(s, nri)).TryRandomElementByWeight(d => d.Worker.SelectionWeightForFaction(map, nri.parms.faction, nri.parms.points), out nri.parms.raidStrategy);
            }
            else
            {
                list.TryRandomElementByWeight(d => d.Worker.SelectionWeightForFaction(map, nri.parms.faction, nri.parms.points), out nri.parms.raidStrategy);
            }

            nri.SetPawnsInfo();
            nri.ChooseAndApplyModifier();
            waveCounter?.UpdateHeight();
            return nri;
        }

        /// <summary>
        /// Check if strategy can be used with parms
        /// </summary>
        internal bool CanUseStrategy(RaidStrategyDef def, NextRaidInfo nri)
        {
            var excluded = WinstonMod.settings.excludedStrategyDefs;
            if (excluded != null && excluded.Contains(def.defName))
                return false;

            if (def == null || !def.Worker.CanUseWith(nri.parms, PawnGroupKindDefOf.Combat))
                return false;

            return def.arriveModes != null && def.arriveModes.Any(x => x.Worker.CanUseWith(nri.parms));
        }

        /// <summary>
        /// Create next normal raid
        /// </summary>
        internal NextRaidInfo SetNextNormalRaidInfo(int ticks, float inDays)
        {
            NextRaidInfo nri = new NextRaidInfo()
            {
                parms = new IncidentParms()
                {
                    target = map,
                    points = GetNextWavePoint(),
                    faction = RandomEnnemyFaction()
                },
                atTick = ticks + (int)(inDays * 60000),
                generatedAt = ticks,
                waveNum = currentWave,
                waveType = currentWave % 5 == 0 ? 1 : 0,
                map = map
            };

            if (!normalStrategies.FindAll(s => CanUseStrategy(s, nri)).TryRandomElementByWeight(d => d.Worker.SelectionWeightForFaction(map, nri.parms.faction, nri.parms.points), out nri.parms.raidStrategy))
                Log.Warning("null raidstrategy");

            nri.SetPawnsInfo();
            nri.ChooseAndApplyModifier();
            waveCounter?.UpdateHeight();
            return nri;
        }

        /// <summary>
        /// Start the raid
        /// </summary>
        internal void StartRaid(int ticks)
        {
            // Send raid
            nextRaidInfo.SendRaid(map, ticks);
            // Send allies if necessary
            if (nextRaidSendAllies)
            {
                nextRaidSendAllies = false;
                // Create parms
                var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                parms.target = map;
                parms.faction = Find.FactionManager.RandomAlliedFaction();
                parms.points = Math.Min(nextRaidInfo.parms.points * 2, WinstonMod.settings.maxPoints);
                // Send raid
                IncidentDefOf.RaidFriendly.Worker.TryExecute(parms);
            }
        }

        /// <summary>
        /// Add hediff increasing stats to pawns
        /// </summary>
        internal void AddStatHediff()
        {
            if (statPawns == null)
                statPawns = new List<Pawn>();

            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.Faction == Faction.OfPlayer && pawn.RaceProps.Humanlike && !statPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(WDefOf.VESWW_IncreasedStats))
                {
                    pawn.health.AddHediff(WDefOf.VESWW_IncreasedStats);
                    statPawns.Add(pawn);
                }
            }
        }

        /// <summary>
        /// Remove hediff increasing stats to pawns
        /// </summary>
        internal void RemoveStatHediff()
        {
            if (statPawns.NullOrEmpty())
                return;

            for (int i = 0; i < statPawns.Count; i++)
            {
                var pawn = statPawns[i];
                if (pawn.health?.hediffSet?.GetFirstHediffOfDef(WDefOf.VESWW_IncreasedStats) is Hediff hediff)
                    pawn.health.RemoveHediff(hediff);
            }

            statPawns.Clear();
        }

        /// <summary>
        /// Register new drop spot
        /// </summary>
        internal void RegisterDropSpot(IntVec3 spot) => dropSpot = spot;

        /// <summary>
        /// Remove ounter from screen
        /// </summary>
        internal void RemoveCounter()
        {
            counterPos = new Vector2(UI.screenWidth - waveCounter.windowRect.xMax, waveCounter.windowRect.y);
            counterDraggable = waveCounter.draggable;
            waveCounter.Close();
            waveCounter = null;
        }
    }
}