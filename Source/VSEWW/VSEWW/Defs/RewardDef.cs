using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class RewardDef : Def
    {
        public string texPath;
        public RewardCategory category;

        public RewardType type = RewardType.Resources;

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

        // Recharge all psyfocus
        public bool rechargePsyfocus = false;

        // Reward honor
        public int rewardHonor = 0;

        // Needs mechanitor in colony
        public bool needsMechanitor = false;

        // Commonality. Lower numbers will appear less often
        public float commonality = 1;

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

        public string Category
        {
            get
            {
                switch (category)
                {
                    case RewardCategory.Normal:
                        return "VESWW.Normal".Translate();
                    case RewardCategory.Good:
                        return "VESWW.Good".Translate();
                    case RewardCategory.Excellent:
                        return "VESWW.Excellent".Translate();
                    case RewardCategory.Legendary:
                        return "VESWW.Legendary".Translate();
                    default:
                        return "VESWW.Poor".Translate();
                }
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in base.ConfigErrors())
                yield return str;
            if (label == null)
                yield return $"RewardDef {defName} has null label";
            if (description == null)
                yield return $"RewardDef {defName} has null description";
            if (texPath == null)
            {
                switch (category)
                {
                    case RewardCategory.Poor:
                        texPath = "UI/Rewards/RewardDefault_Poor";
                        break;

                    case RewardCategory.Normal:
                        texPath = "UI/Rewards/RewardDefault_Normal";
                        break;

                    case RewardCategory.Good:
                        texPath = "UI/Rewards/RewardDefault_Good";
                        break;

                    case RewardCategory.Excellent:
                        texPath = "UI/Rewards/RewardDefault_Excellent";
                        break;

                    case RewardCategory.Legendary:
                        texPath = "UI/Rewards/RewardDefault_Legendary";
                        break;

                    default:
                        break;
                }
            }
        }

        public void DrawCard(Rect rect, Window_ChooseReward window)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width);
            GUI.DrawTexture(iconRect.ContractedBy(20), RewardIcon);

            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperCenter;

            Text.Font = GameFont.Small;
            Rect labelRect = new Rect(rect.x, iconRect.yMax + 5, rect.width, 20);
            Widgets.Label(labelRect, label);

            Text.Font = GameFont.Tiny;
            Rect catRect = new Rect(rect.x, labelRect.yMax + 10, rect.width, 20);
            Widgets.Label(catRect, "VESWW.Reward".Translate(Category));

            Rect descRect = new Rect(rect.x, catRect.yMax + 5, rect.width, 70);
            Widgets.Label(descRect.ContractedBy(5), description);

            Rect buttonRect = new Rect(rect.x, rect.yMax - 35, rect.width, 30);
            Rect buttonRectB = buttonRect.ContractedBy(5);
            if (Widgets.ButtonText(buttonRectB, "VESWW.SelectReward".Translate()))
            {
                window.choosenReward = this;
                window.Close();
            }
            Text.Font = GameFont.Small;
            Text.Anchor = anchor;
        }
    }
}