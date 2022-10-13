using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class Window_ChooseReward : Window
    {
        internal RewardDef choosenReward;

        private readonly int margin = 10;
        private readonly Map map;

        private readonly Dictionary<RewardCategory, int> commonalities;
        private readonly List<RewardDef> rewardPool;
        private readonly float fourthRewardChance;

        private List<RewardDef> rewards;
        private int rewardNumber = 3;
        private int width = 750;

        internal Window_ChooseReward(int waveNumber, float fourthRewardChance, Map map)
        {
            commonalities = RewardCommonalities.GetCommonalities(waveNumber);
            if (Startup.NoPauseChallengeActive)
            {
                forcePause = false;
                preventSave = false;
            }
            else
            {
                forcePause = true;
                preventSave = true;
            }
            absorbInputAroundWindow = true;
            doCloseX = false;
            doCloseButton = false;
            closeOnClickedOutside = false;
            closeOnCancel = false;
            doWindowBackground = false;
            drawShadow = false;

            this.map = map;
            this.fourthRewardChance = fourthRewardChance;

            rewardPool = DefDatabase<RewardDef>.AllDefsListForReading.ToList();
            if (Find.FactionManager.RandomAlliedFaction() == null)
                rewardPool.RemoveAll(r => r.waveModifier?.allies == true);
            if (!DefDatabase<ResearchProjectDef>.AllDefsListForReading.FindAll(x => x.CanStartNow).Any())
                rewardPool.RemoveAll(r => r.unlockXResearch != 0);
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
                    rewards.ElementAt(i).DrawCard(r, this);
                    lastMaxX = r.xMax;
                }
            }
            else if (!WinstonMod.settings.randomRewardMod)
            {
                if (new System.Random().NextDouble() < fourthRewardChance)
                    rewardNumber++;

                width /= rewardNumber;
                rewards = new List<RewardDef>();
                for (int i = 0; i < rewardNumber; i++)
                {
                    var reward = rewardPool.FindAll(r => r.category == commonalities.RandomElementByWeight(k => k.Value).Key).RandomElement();
                    rewards.Add(reward);
                    rewardPool.Remove(reward);
                }
            }
            else
            {
                choosenReward = rewardPool.FindAll(r => r.category == commonalities.RandomElementByWeight(k => k.Value).Key).RandomElement();
                Close();
            }
        }

        public override void PostClose()
        {
            if (WinstonMod.settings.randomRewardMod)
                Messages.Message("VESWW.RandRewardOutcome".Translate(choosenReward.LabelCap), MessageTypeDefOf.NeutralEvent);

            RewardCreator.SendReward(choosenReward, map);
        }
    }
}