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
        private Dictionary<RewardCategory, int> commonality;
        private List<RewardDef> rewards;
        private int margin = 10;
        private int width = 250;

        public Window_ChooseReward(int waveNumber)
        {
            this.commonality = RewardCategoryExtension.GetCommonality(waveNumber);
            this.forcePause = true;
            this.doCloseX = false;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = false;
            this.doWindowBackground = false;
            this.drawShadow = false;
            this.preventSave = true;

            rewards = new List<RewardDef>();
            for (int i = 0; i < 3; i++)
            {
                rewards.Add(DefDatabase<RewardDef>.AllDefsListForReading.FindAll(r => r.category == commonality.RandomElementByWeight(k => k.Value).Key).RandomElement());
            }
        }

        public override Vector2 InitialSize => new Vector2(850f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect rOne = new Rect(0, 0, width, inRect.height);
            Widgets.DrawWindowBackground(rOne);
            rewards.ElementAt(0).DrawCard(rOne, this, Find.CurrentMap);

            Rect rTwo = new Rect(rOne.xMax + margin, 0, width, inRect.height);
            Widgets.DrawWindowBackground(rTwo);
            rewards.ElementAt(1).DrawCard(rTwo, this, Find.CurrentMap);

            Rect rThree = new Rect(rTwo.xMax + margin, 0, width, inRect.height);
            Widgets.DrawWindowBackground(rThree);
            rewards.ElementAt(2).DrawCard(rThree, this, Find.CurrentMap);
        }
    }
}
