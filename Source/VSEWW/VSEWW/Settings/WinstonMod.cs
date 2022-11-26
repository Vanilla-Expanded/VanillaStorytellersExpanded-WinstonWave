using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class WinstonMod : Mod
    {
        private string _timeBeforeFirstWave;
        private string _timeBetweenWaves;
        private string _timeToDefeatWave;
        private string _maxPoints;
        private string _pointMultiplierBefore;
        private string _pointMultiplierAfter;

        private const float _fullHeight = 590;

        private Vector2 _scrollPosition;

        public static WinstonSettings settings;

        public WinstonMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<WinstonSettings>();
        }

        public int currentTab = 0;

        public override string SettingsCategory() => "VESWW.ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect tabRect = new Rect(inRect)
            {
                y = inRect.y + 40f
            };
            Rect mainRect = new Rect(inRect)
            {
                height = inRect.height - 40f,
                y = inRect.y + 40f
            };

            Widgets.DrawMenuSection(mainRect);
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("VESWW.GS".Translate(), () =>
                {
                    currentTab = 0;
                    WriteSettings();
                }, currentTab == 0),
                new TabRecord("VESWW.VS".Translate(), () =>
                {
                    currentTab = 1;
                    WriteSettings();
                }, currentTab == 1),
                new TabRecord("VESWW.MS".Translate(), () =>
                {
                    currentTab = 2;
                    WriteSettings();
                }, currentTab == 2)
            };
            TabDrawer.DrawTabs(tabRect, tabs);

            if (currentTab == 0)
            {
                DoGameSettings(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 1)
            {
                DoWaveSettings(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 2)
            {
                DoModifierSettings(mainRect.ContractedBy(15f));
            }
        }

        private void DoModifierSettings(Rect rect)
        {
            var modSettingsLst = new Listing_Standard();
            modSettingsLst.Begin(rect);

            modSettingsLst.Label("VESWW.Modifiers".Translate());
            if (modSettingsLst.ButtonText("VESWW.AModifiers".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.modifierDefs.NullOrEmpty())
                    settings.modifierDefs = new List<string>();

                foreach (var item in DefDatabase<ModifierDef>.AllDefsListForReading.FindAll(m => !settings.modifierDefs.Contains(m.defName)))
                {
                    floatMenuOptions.Add(new FloatMenuOption($"{item.LabelCap}", () => settings.modifierDefs.Add(item.defName)));
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to add", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            modSettingsLst.Gap(5);
            if (modSettingsLst.ButtonText("VESWW.RModifiers".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (!settings.modifierDefs.NullOrEmpty())
                {
                    foreach (var item in settings.modifierDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item, () => settings.modifierDefs.Remove(item)));
                    }
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            modSettingsLst.End();
        }

        private void DoWaveSettings(Rect rect)
        {
            var fullRect = new Rect(rect)
            {
                height = _fullHeight
            };

            Widgets.BeginScrollView(rect, ref _scrollPosition, fullRect, false);
            var waveSettingsLst = new Listing_Standard();
            waveSettingsLst.Begin(fullRect);

            waveSettingsLst.Label("VESWW.TimeBeforeFirstWave".Translate(), tooltip: "VESWW.TimeBeforeFirstWaveTip".Translate());
            waveSettingsLst.TextFieldNumeric(ref settings.timeBeforeFirstWave, ref _timeBeforeFirstWave, 1f, 10f);
            waveSettingsLst.Gap(5);

            waveSettingsLst.Label("VESWW.TimeBetweenWaves".Translate(), tooltip: "VESWW.TimeBetweenWavesTip".Translate());
            waveSettingsLst.TextFieldNumeric(ref settings.timeBetweenWaves, ref _timeBetweenWaves, 1f, 10f);
            waveSettingsLst.Gap(5);

            waveSettingsLst.Label("VESWW.TimeToDefeatWave".Translate(), tooltip: "VESWW.TimeToDefeatWaveTip".Translate());
            waveSettingsLst.TextFieldNumeric(ref settings.timeToDefeatWave, ref _timeToDefeatWave, 1f, 10f);
            waveSettingsLst.GapLine(12);

            waveSettingsLst.Gap(12);
            waveSettingsLst.Label("VESWW.MaxPoints".Translate(), tooltip: "VESWW.MaxPointsTip".Translate());
            waveSettingsLst.IntEntry(ref settings.maxPoints, ref _maxPoints, 10);
            waveSettingsLst.Gap(5);

            waveSettingsLst.Label("VESWW.PointMultiplierBefore20".Translate(), tooltip: "VESWW.PointMultiplierBefore20Tip".Translate());
            waveSettingsLst.TextFieldNumeric(ref settings.pointMultiplierBefore, ref _pointMultiplierBefore, 1f, 10f);
            waveSettingsLst.Gap(5);

            waveSettingsLst.Label("VESWW.PointMultiplierAfter20".Translate(), tooltip: "VESWW.PointMultiplierAfter20Tip".Translate());
            waveSettingsLst.TextFieldNumeric(ref settings.pointMultiplierAfter, ref _pointMultiplierAfter, 1f, 10f);
            waveSettingsLst.GapLine(12);

            waveSettingsLst.Gap(12);
            waveSettingsLst.Label("VESWW.ExcludedFaction".Translate());
            if (waveSettingsLst.ButtonText("VESWW.AddExcludedFaction".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.excludedFactionDefs.NullOrEmpty())
                    settings.excludedFactionDefs = new List<string>();

                foreach (var item in DefDatabase<FactionDef>.AllDefsListForReading.FindAll(f => !f.pawnGroupMakers.NullOrEmpty()
                                                                                                && !settings.excludedFactionDefs.Contains(f.defName)
                                                                                                && f.pawnGroupMakers != null
                                                                                                && f.pawnGroupMakers.Any(p => p.kindDef == PawnGroupKindDefOf.Combat)))
                {
                    floatMenuOptions.Add(new FloatMenuOption($"{item.LabelCap} ({item.defName})", () => settings.excludedFactionDefs.Add(item.defName)));
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to add", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            waveSettingsLst.Gap(5);
            if (waveSettingsLst.ButtonText("VESWW.RemoveExcludedFaction".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (!settings.excludedFactionDefs.NullOrEmpty())
                {
                    foreach (var item in settings.excludedFactionDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item, () => settings.excludedFactionDefs.Remove(item)));
                    }
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            waveSettingsLst.GapLine(12);

            waveSettingsLst.Gap(12);
            waveSettingsLst.Label("VESWW.ExcludedStrategy".Translate());
            if (waveSettingsLst.ButtonText("VESWW.AddExcludedStrategy".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.excludedStrategyDefs.NullOrEmpty())
                    settings.excludedStrategyDefs = new List<string>();

                foreach (var item in DefDatabase<RaidStrategyDef>.AllDefsListForReading.FindAll(f => f.arrivalTextEnemy != null
                                                                                                     && !MapComponent_Winston.normalStrategies.Contains(f)
                                                                                                     && !settings.excludedStrategyDefs.Contains(f.defName)))
                {
                    floatMenuOptions.Add(new FloatMenuOption($"{item.defName}", () => settings.excludedStrategyDefs.Add(item.defName)));
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to add", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            waveSettingsLst.Gap(5);
            if (waveSettingsLst.ButtonText("VESWW.RemoveExcludedStrategy".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (!settings.excludedStrategyDefs.NullOrEmpty())
                {
                    foreach (var item in settings.excludedStrategyDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item, () => settings.excludedStrategyDefs.Remove(item)));
                    }
                }

                if (floatMenuOptions.Count == 0) floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            waveSettingsLst.End();
            Widgets.EndScrollView();
        }

        private void DoGameSettings(Rect rect)
        {
            var gameSettingsLst = new Listing_Standard();
            gameSettingsLst.Begin(rect);

            gameSettingsLst.CheckboxLabeled("VESWW.MysteryMod".Translate(), ref settings.mysteryMod, "VESWW.MysteryModTip".Translate());
            gameSettingsLst.Gap(5);

            gameSettingsLst.CheckboxLabeled("VESWW.DontShowPawnList".Translate(), ref settings.showPawnList);
            gameSettingsLst.Gap(5);

            gameSettingsLst.CheckboxLabeled("VESWW.RandomRewardMod".Translate(), ref settings.randomRewardMod, "VESWW.RandomRewardModTip".Translate());
            gameSettingsLst.Gap(5);

            gameSettingsLst.CheckboxLabeled("VESWW.EnableStats".Translate(), ref settings.enableStatIncrease, "VESWW.EnableStatsTip".Translate());
            gameSettingsLst.GapLine(12);

            gameSettingsLst.Gap(12);
            gameSettingsLst.CheckboxLabeled("VESWW.DrawBack".Translate(), ref settings.drawBackground);
            gameSettingsLst.Gap(5);

            gameSettingsLst.CheckboxLabeled("VESWW.ShowDraggable".Translate(), ref settings.hideToggleDraggable, "VESWW.ShowDraggableTip".Translate());
            gameSettingsLst.Gap(5);

            gameSettingsLst.CheckboxLabeled("VESWW.UseRimworldTime".Translate(), ref settings.useRimworldTime, "VESWW.UseRimworldTimeTip".Translate());
            gameSettingsLst.Gap(5);

            if (gameSettingsLst.ButtonText("VESWW.ResetCounterPos".Translate()))
            {
                if (Find.CurrentMap != null && Find.CurrentMap.GetComponent<MapComponent_Winston>() is MapComponent_Winston mcW)
                {
                    mcW.counterPos = Vector2.zero;
                    mcW.waveCounter.UpdateWindow();
                }
                else
                {
                    Messages.Message("VESWW.LoadToReset".Translate(), MessageTypeDefOf.NeutralEvent, false);
                }
            }
            gameSettingsLst.GapLine(12);

            gameSettingsLst.Gap(12);
            gameSettingsLst.CheckboxLabeled("VESWW.DisableSteelSlagChunk".Translate(), ref settings.dropSlagChunk);
            gameSettingsLst.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            var mapComp = Find.CurrentMap?.GetComponent<MapComponent_Winston>();
            if (mapComp == null)
                return;

            if (settings.enableStatIncrease)
                mapComp.AddStatHediff();
            else
                mapComp.RemoveStatHediff();

            mapComp.waveCounter.UpdateWindow();
        }
    }
}
