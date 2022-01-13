using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class Window_ChooseReward : Window
    {
        private readonly Dictionary<RewardCategory, int> commonality;
        private readonly List<RewardDef> rewards;
        private readonly int margin = 10;
        private readonly int width = 750;
        private readonly int rewardNumber = 3;

        public Window_ChooseReward(int waveNumber, float fourthRewardChance)
        {
            this.commonality = RewardCategoryExtension.GetCommonality(waveNumber);
            this.forcePause = true;
            this.doCloseX = false;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.closeOnCancel = false;
            this.absorbInputAroundWindow = true;
            this.doWindowBackground = false;
            this.drawShadow = false;
            this.preventSave = true;


            if (new System.Random().NextDouble() < fourthRewardChance) { rewardNumber++; }

            width /= rewardNumber;

            rewards = new List<RewardDef>();
            for (int i = 0; i < rewardNumber; i++)
            {
                rewards.Add(DefDatabase<RewardDef>.AllDefsListForReading.FindAll(r => r.category == commonality.RandomElementByWeight(k => k.Value).Key && !rewards.Contains(r)).RandomElement());
            }
        }

        public override Vector2 InitialSize => new Vector2(850f, 500f);

        public override void DoWindowContents(Rect inRect)
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
    }
}
