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
    }
}
