using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public static class DebugOptions
    {
        [DebugAction("VES Winston Wave", "Rewards test", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RewardTest()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
            List<RewardDef> rewards = DefDatabase<RewardDef>.AllDefsListForReading;

            foreach (var r in rewards)
            {
                debugMenuOptionList.Add(new DebugMenuOption(r.defName.Remove(0, 6), DebugMenuOptionMode.Action, () => RewardCreator.SendReward(r, Find.CurrentMap)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }

        [DebugAction("VES Winston Wave", "Send all rewards", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SendAllReward()
        {
            foreach (var r in DefDatabase<RewardDef>.AllDefsListForReading)
            {
                RewardCreator.SendReward(r, Find.CurrentMap);
            }
        }

        [DebugAction("VES Winston Wave", "Skip to wave...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SkipToWave()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
            List<RewardDef> rewards = DefDatabase<RewardDef>.AllDefsListForReading;

            for (int i = 1; i <= 15; i++)
            {
                int waveNum = i * 10;
                debugMenuOptionList.Add(new DebugMenuOption(waveNum.ToString(), DebugMenuOptionMode.Action, () =>
                {
                    var c = Find.CurrentMap.GetComponent<MapComponent_Winston>();
                    for (int w = 0; w < waveNum; w++)
                    {
                        c.currentWave++;
                        c.GetNextWavePoint();
                        Log.Message($"{c.currentWave}:{c.currentPoints}");
                    }
                    c.nextRaidInfo = c.currentWave % 5 == 0 ? c.SetNextBossRaidInfo(1) : c.SetNextNormalRaidInfo(1);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }
    }
}
