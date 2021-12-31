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
	public class RewardDef : Def
    {
        public string texPath;
        public RewardCategory category;
		// Send reward of specific category : everything else will be ignored. 
		// We never send a random poor reward
		public RewardCategory sendRewardOf = RewardCategory.Poor;
		// Specific pawnkind
		public List<PawnReward> pawns;
		// Random pawns
		public List<RPawnReward> randomPawns;
		// Specific items
		public List<ItemReward> items;
		// Random items of categories
		public List<RItemReward> randomItems;
		// Send specific incident
		public IncidentDef incidentDef;
		// Skills boost
		public int boostSkillBy = 0;
		// Unlock X research projects
		public int unlockXResearch = 0;
		// Mass heal all colony pawns (colonists, slaves, prisonners, animals)
		public bool massHeal = false;
		// Modify waves
		public WaveModifier waveModifier;

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
			GUI.DrawTexture(iconRect.ContractedBy(10), RewardIcon);

			var anchor = Text.Anchor;
			Text.Anchor = TextAnchor.UpperCenter;

			Text.Font = GameFont.Small;
			Rect labelRect = new Rect(rect.x, iconRect.yMax + 5, rect.width, 20);
			Widgets.Label(labelRect, label);

			Text.Font = GameFont.Tiny;
			Rect catRect = new Rect(rect.x, labelRect.yMax + 10, rect.width, 20);
			Widgets.Label(catRect, "VESWW.Reward".Translate(category.ToString()));

			Rect descRect = new Rect(rect.x, catRect.yMax + 5, rect.width, 70);
			Widgets.Label(descRect.ContractedBy(5), description);

			Rect buttonRect = new Rect(rect.x, rect.yMax - 35, rect.width, 30);
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
