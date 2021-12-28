using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace VSEWW
{
    public abstract class RewardDef : Def
    {
        public string texPath;
        public RewardCategory category;

        private Texture2D rewardIcon;

		public Texture2D RewardIcon
		{
			get
			{
				if (rewardIcon is null)
				{
					rewardIcon = texPath == null ? null : ContentFinder<Texture2D>.Get(texPath, false);
					if (rewardIcon is null)
					{
						rewardIcon = BaseContent.BadTex;
					}
				}
				return rewardIcon;
			}
		}

		public virtual void DrawCard(Rect rect, Window window, Map map)
		{
			Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width);
			Rect innerIconRect = iconRect.ContractedBy(10);
			GUI.DrawTexture(innerIconRect, RewardIcon); // yMax = 200

			var anchor = Text.Anchor;
			Text.Anchor = TextAnchor.UpperCenter;

			Text.Font = GameFont.Small;
			Rect labelRect = new Rect(rect.x, iconRect.yMax + 5, rect.width, 20); // yMax = 225
			Widgets.Label(labelRect, label);

			Text.Font = GameFont.Tiny;
			Rect catRect = new Rect(rect.x, labelRect.yMax + 10, rect.width, 20); // yMax = 255
			Widgets.Label(catRect, "VESWW.Reward".Translate(category.ToString()));

			Rect descRect = new Rect(rect.x, catRect.yMax + 5, rect.width, 70); // yMax = 330
			Widgets.Label(descRect, description);

			Rect buttonRect = new Rect(rect.x, descRect.yMax + 5, rect.width, 30); // yMax = 365
			Rect buttonRectB = buttonRect.ContractedBy(5);
			if (Widgets.ButtonText(buttonRectB, "VESWW.SelectReward".Translate()))
            {
				RewardCreator.SendReward(this, map);
				window.Close();
            }
			Text.Font = GameFont.Small;
			Text.Anchor = anchor;
		}
	}
}
