using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class Dialog_FactionMultipliers : Window
    {
        private Vector2 scrollPosition;
        private List<FactionDef> factions;
        private const float RowHeight = 160f;
        private readonly bool gameActive;

        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public Dialog_FactionMultipliers()
        {
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;

            gameActive = Current.ProgramState == ProgramState.Playing && Find.FactionManager != null;

            factions = DefDatabase<FactionDef>.AllDefsListForReading
                .Where(f => !f.pawnGroupMakers.NullOrEmpty()
                    && f.pawnGroupMakers.Any(p => p.kindDef == PawnGroupKindDefOf.Combat)
                    && (WinstonMod.settings.excludedFactionDefs == null || !WinstonMod.settings.excludedFactionDefs.Contains(f.defName)))
                .OrderBy(f => f.label)
                .ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var titleRect = new Rect(inRect) { height = 30f };
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VESWW.FactionMultipliers".Translate());
            Text.Font = GameFont.Small;

            var outerRect = new Rect(inRect) { y = inRect.y + 35f, height = inRect.height - 75f };
            var viewWidth = outerRect.width - 16f;
            var innerRect = new Rect(0f, 0f, viewWidth, factions.Count * RowHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, innerRect);

            var listing = new Listing_Standard();
            listing.Begin(innerRect);

            foreach (var factionDef in factions)
            {
                float weightMult = WinstonMod.settings.GetFactionWeightMultiplier(factionDef.defName);
                float pointsMult = WinstonMod.settings.GetFactionPointsMultiplier(factionDef.defName);

                var nameRect = listing.GetRect(Text.LineHeight);
                Text.WordWrap = false;

                string label = $"{factionDef.LabelCap} ({factionDef.defName})";

                if (gameActive)
                {
                    var factionInstance = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def == factionDef);

                    string relationLabel;
                    if (factionInstance == null)
                        relationLabel = "VESWW.Unknown".Translate();
                    else if (factionInstance.HostileTo(Faction.OfPlayer))
                        relationLabel = "VESWW.Hostile".Translate();
                    else if (factionInstance.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally)
                        relationLabel = "VESWW.Allied".Translate();
                    else
                        relationLabel = "VESWW.Neutral".Translate();

                    label += $" [{relationLabel}]";
                }

                Widgets.Label(nameRect, label);
                Text.WordWrap = true;

                listing.Label($"{"VESWW.WeightMultiplier".Translate()}: {weightMult:F1}");
                weightMult = listing.Slider(weightMult, 0f, 10f);
                WinstonMod.settings.factionWeightMultipliers[factionDef.defName] = weightMult;

                listing.Label($"{"VESWW.StrengthMultiplier".Translate()}: {pointsMult:F1}");
                pointsMult = listing.Slider(pointsMult, 0f, 10f);
                WinstonMod.settings.factionPointsMultipliers[factionDef.defName] = pointsMult;

                listing.GapLine(8f);
            }

            listing.End();
            Widgets.EndScrollView();
        }

        public override void PreClose()
        {
            base.PreClose();
            WinstonMod.settings.Write();
        }
    }
}