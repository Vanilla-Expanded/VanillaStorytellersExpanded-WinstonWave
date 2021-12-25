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
        public bool sent;
        public int atTick;
        public List<string> modifiers = new List<string>();
        public IncidentParms incidentParms;
        public Dictionary<PawnKindDef, int> pawnKinds = new Dictionary<PawnKindDef, int>();

        private Lord lord;

        public void ExposeData()
        {
            Scribe_Values.Look(ref sent, "sent");
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Collections.Look(ref pawnKinds, "pawnKinds");
            Scribe_Deep.Look(ref incidentParms, "incidentParms");
        }

        public string TimeBeforeWave() => TimeSpan.FromSeconds((atTick - Find.TickManager.TicksGame).TicksToSeconds()).ToString();

        public int WavePawnsLeft()
        {
            if (sent)
            {
                if (lord == null)
                {
                    Map map = (Map)incidentParms.target;
                    foreach (Lord lord in map.lordManager.lords)
                    {
                        if (lord.faction != null && lord.faction == incidentParms.faction && lord.ownedPawns != null)
                        {
                            if (lord.ownedPawns.All((p) => { return p.mindState.Active; }) && lord.ownedPawns.Exists(p => p.Map != null && !p.Map.fogGrid.IsFogged(p.Position)))
                            {
                                this.lord = lord;
                            }
                        }
                    }
                }

                return lord.ownedPawns.FindAll(p => p.CurJobDef != JobDefOf.Flee).Count;
            }
            else
            {
                Log.Error("Trying to access pawn left with not launched wave");
                return 0;
            }
        }

        public string ToStringReadable()
        {
            if (pawnKinds == null) Log.Message("Null pawnKinds");
            if (incidentParms == null) Log.Message("Null parms");
            if (incidentParms.faction == null) Log.Message("Null faction");
            if (incidentParms.raidStrategy == null) Log.Message("Null raidStrategy");
            if (incidentParms.raidArrivalMode == null) Log.Message("Null raidArrivalMode");

            string str = $"Raid from {incidentParms.faction.Name} with {pawnKinds.Sum(k => k.Value)} pawns. Using {incidentParms.raidStrategy.defName} and {incidentParms.raidArrivalMode.defName}\n";
            // Add it to the string
            foreach (var pair in pawnKinds)
            {
                str += $"{pair.Value} {pair.Key.label} ";
            }
            return str + $"\n{TimeBeforeWave()}";
        }
    }

    internal class MapComponent_Winston : MapComponent
    {
        int currentWave = 1;
        float currentPoints = 0;
        public NextRaidInfo nextRaidInfo;

        public MapComponent_Winston(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentWave, "currentWave");
            Scribe_Values.Look(ref currentPoints, "currentPoints");
            Scribe_Deep.Look(ref nextRaidInfo, "nextRaidInfo");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            nextRaidInfo = SetNextNormalRaidInfo(VESWWMod.settings.timeBeforeFirstWave);
            Log.Message(nextRaidInfo.ToStringReadable());
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                if (nextRaidInfo != null)
                {
                    if (!nextRaidInfo.sent && nextRaidInfo.atTick <= Find.TickManager.TicksGame)
                    {
                        ExecuteRaid();
                        nextRaidInfo.sent = true;
                    }
                }
                else
                {
                    nextRaidInfo = SetNextNormalRaidInfo(VESWWMod.settings.timeBetweenWaves);
                }
            }
        }

        private void ExecuteRaid()
        {
            ++Find.StoryWatcher.statsRecord.numRaidsEnemy;
            map.StoryState.lastRaidFaction = nextRaidInfo.incidentParms.faction;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame, nextRaidInfo.incidentParms);
        }

        private NextRaidInfo SetNextNormalRaidInfo(int inDays)
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
                atTick = Find.TickManager.TicksGame + (inDays * 60000)
            };
            nri.incidentParms.raidStrategy = from.Find(s => s.Worker.CanUseWith(nri.incidentParms, PawnGroupKindDefOf.Combat));

            var pList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, nri.incidentParms)).ToList();
            // Get all kinds and the number of them
            foreach (Pawn pawn in pList)
            {
                nri.pawnKinds.SetOrAdd(pawn.kindDef, 1);
            }

            return nri;
        }

        private float GetNextWavePoint()
        {
            if (currentPoints <= 0) currentPoints = 100f;
            else currentPoints *= 1.2f;

            return currentPoints;
        }
    }
}
