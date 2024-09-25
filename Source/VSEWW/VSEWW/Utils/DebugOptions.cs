using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using LudeonTK;

namespace VSEWW
{
    public static class DebugOptions
    {
        [DebugAction("VES Winston Wave", "Rewards test", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RewardTest()
        {
            var debugMenuOptionList = new List<DebugMenuOption>();
            var rewards = DefDatabase<RewardDef>.AllDefsListForReading;
            var map = Find.CurrentMap;
            var comp = map.GetComponent<MapComponent_Winston>();

            foreach (var r in rewards)
            {
                debugMenuOptionList.Add(new DebugMenuOption(r.defName.Remove(0, 6), DebugMenuOptionMode.Action, () => RewardCreator.SendReward(r, map, comp)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }

        [DebugAction("VES Winston Wave", "Send all rewards", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SendAllReward()
        {
            var map = Find.CurrentMap;
            var comp = map.GetComponent<MapComponent_Winston>();
            foreach (var r in DefDatabase<RewardDef>.AllDefsListForReading)
            {
                RewardCreator.SendReward(r, map, comp);
            }
        }

        [DebugAction("VES Winston Wave", "Skip to wave...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SkipToWave()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
            List<RewardDef> rewards = DefDatabase<RewardDef>.AllDefsListForReading;

            for (int i = 1; i <= 20; i++)
            {
                int waveNum = i * 5;
                debugMenuOptionList.Add(new DebugMenuOption(waveNum.ToString(), DebugMenuOptionMode.Action, () =>
                {
                    var ticksGame = Find.TickManager.TicksGame;
                    var c = Find.CurrentMap.GetComponent<MapComponent_Winston>();
                    if(c != null && Find.Storyteller.def.defName== "VSE_WinstonWave") {
                        for (int w = c.currentWave; w < waveNum; w++)
                        {
                            c.currentWave++;
                            c.GetNextWavePoint();
                        }
                        c.nextRaidInfo = new NextRaidInfo();
                        c.nextRaidInfo.Init(waveNum, c.GetNextWavePoint(), c.map);
                        c.waveCounter.UpdateWindow();
                        c.waveCounter.WaveTip();
                    }
                    
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }

        [DebugAction("VES Winston Wave", "Send wave now", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SendWaveNow()
        {
            var c = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            if (c != null && Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                c.nextRaidInfo.atTick = Find.TickManager.TicksGame + 20;
            }
            
        }

        [DebugAction("VES Winston Wave", "Add modifier to wave", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddModifier()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();

            var c = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            if (c != null && Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                foreach (var m in DefDatabase<ModifierDef>.AllDefsListForReading)
                {
                    if (m.defName == "VSEWW_DoubleTrouble")
                        continue;

                    debugMenuOptionList.Add(new DebugMenuOption(m.label, DebugMenuOptionMode.Action, () =>
                    {
                        if (c.nextRaidInfo.modifiers.Count < 2 && !c.nextRaidInfo.modifiers.Any(mo => mo.incompatibleWith.Contains(m)))
                        {
                            c.nextRaidInfo.modifiers.Add(m);
                            c.nextRaidInfo.ApplyPrePawnGen();
                            c.nextRaidInfo.SetPawnsInfo();
                            c.nextRaidInfo.ApplyPostPawnGen();
                            c.waveCounter.UpdateWindow();
                        }
                        else
                        {
                            Messages.Message("Cannot add this modifier to this wave", MessageTypeDefOf.CautionInput);
                        }
                    }));
                }
            
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }

        [DebugAction("VES Winston Wave", "Reset wave", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetWave()
        {
            var mapComp = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            if (mapComp != null && Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                mapComp.nextRaidInfo.StopIncidentModifiers();
                mapComp.nextRaidInfo = new NextRaidInfo();
                mapComp.nextRaidInfo.Init(mapComp.currentWave, mapComp.currentPoints, mapComp.map);
                mapComp.waveCounter.UpdateWindow();
            }
            

        }

        [DebugAction("VES Winston Wave", "Reset to wave 1", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetToWaveOne()
        {
            var mapComp = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            if (mapComp != null && Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                mapComp.currentWave = 1;
                mapComp.currentPoints = 1;

                mapComp.nextRaidInfo.StopIncidentModifiers();
                mapComp.nextRaidInfo = new NextRaidInfo();
                mapComp.nextRaidInfo.Init(mapComp.currentWave, mapComp.GetNextWavePoint(), mapComp.map);
                mapComp.waveCounter.UpdateWindow();
            }
        }
    }
}