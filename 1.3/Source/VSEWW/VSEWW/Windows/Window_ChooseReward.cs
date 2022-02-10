using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class Window_ChooseReward : Window
    {
        internal RewardDef choosenReward;
        
        private readonly Dictionary<RewardCategory, int> commonality;
        private readonly List<RewardDef> rewards;
        private readonly int margin = 10;
        private readonly int width = 750;
        private readonly int rewardNumber = 3;
        private Map map;
        private MapComponent_Winston mapComp;

        internal Window_ChooseReward(int waveNumber, float fourthRewardChance, Map map, MapComponent_Winston mapComp)
        {
            commonality = RewardCategoryExtension.GetCommonality(waveNumber);
            // if (!LoadedModManager.RunningMods.Any(m => m.PackageId == "brrainz.nopausechallenge"))
            forcePause = true;
            doCloseX = false;
            doCloseButton = false;
            closeOnClickedOutside = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            doWindowBackground = false;
            drawShadow = false;
            preventSave = true;
            this.map = map;
            this.mapComp = mapComp;

            var rewardPool = DefDatabase<RewardDef>.AllDefsListForReading.ToList();
            if (Find.FactionManager.RandomAlliedFaction() == null)
                rewardPool.RemoveAll(r => r.waveModifier?.allies == true);

            if (!VESWWMod.settings.randomRewardMod)
            {
                if (new System.Random().NextDouble() < fourthRewardChance)
                    rewardNumber++;

                width /= rewardNumber;
                rewards = new List<RewardDef>();
                for (int i = 0; i < rewardNumber; i++)
                {
                    var reward = rewardPool.FindAll(r => r.category == commonality.RandomElementByWeight(k => k.Value).Key).RandomElement();
                    rewards.Add(reward);
                    rewardPool.Remove(reward);
                }
            }
            else
            {
                choosenReward = rewardPool.FindAll(r => r.category == commonality.RandomElementByWeight(k => k.Value).Key).RandomElement();
            }
        }

        public override Vector2 InitialSize => new Vector2(850f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            if (!rewards.NullOrEmpty())
            {
                float lastMaxX = 0f;
                for (int i = 0; i < rewards.Count; i++)
                {
                    Rect r = new Rect(lastMaxX + (i > 0 ? margin : 0), 0, width, inRect.height).Rounded();
                    Widgets.DrawWindowBackground(r);
                    rewards.ElementAt(i).DrawCard(r, this, Find.CurrentMap);
                    lastMaxX = r.xMax;
                }
            }
            else
            {
                Close();
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            mapComp.nextRaidInfo.StopEvents();
            if (VESWWMod.settings.randomRewardMod)
            {
                Messages.Message("VESWW.RandRewardOutcome".Translate(choosenReward.LabelCap), MessageTypeDefOf.NeutralEvent);
            }

            RewardCreator.SendReward(choosenReward, map);
            var delay = choosenReward.waveModifier != null ? choosenReward.waveModifier.delayBy : 0f;

            if (++mapComp.currentWave % 5 == 0)
                mapComp.nextRaidInfo = mapComp.SetNextBossRaidInfo(VESWWMod.settings.timeBetweenWaves + delay);
            else
                mapComp.nextRaidInfo = mapComp.SetNextNormalRaidInfo(VESWWMod.settings.timeBetweenWaves + delay);

            mapComp.waveCounter?.UpdateHeight();
            mapComp.waveCounter?.UpdateWidth();
            mapComp.waveCounter?.WaveTip();
        }
    }
}
