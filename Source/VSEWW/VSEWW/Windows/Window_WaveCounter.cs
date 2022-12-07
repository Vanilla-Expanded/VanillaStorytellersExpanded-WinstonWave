using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    internal class Window_WaveCounter : Window
    {
        private static readonly Color WindowBGColor = new ColorInt(21, 25, 29, 64).ToColor;

        public override Vector2 InitialSize => new Vector2(150f, 200f);

        const int ModifierSize = 50;

        internal MapComponent_Winston mcw;
        internal string waveTip;
        internal Vector2 pos;

        public Window_WaveCounter(MapComponent_Winston mapComponent_Winston, bool counterDraggable, Vector2 pos)
        {
            mcw = mapComponent_Winston;
            this.pos = pos;
            forcePause = false;
            absorbInputAroundWindow = false;
            closeOnCancel = false;
            closeOnClickedOutside = false;
            doCloseButton = false;
            doCloseX = false;
            draggable = counterDraggable;
            preventCameraMotion = false;
            resizeable = false;
            doWindowBackground = false;
            drawShadow = false;
            layer = WindowLayer.GameUI;

            WaveTip();
        }

        public override void WindowOnGUI()
        {
            if (!WorldRendererUtility.WorldRenderedNow) base.WindowOnGUI();
        }

        public override void PostClose()
        {
            base.PostClose();
            mcw.waveCounter = null;
        }

        public override void Notify_ResolutionChanged()
        {
            UpdateWindow();
        }

        public void UpdateWindow()
        {
            // Manage height
            if (mcw.nextRaidInfo.sent)
                windowRect.height = 135f + (mcw.nextRaidInfo.kindListLines * 16f);
            else
                windowRect.height = 160f;

            if (!WinstonMod.settings.hideToggleDraggable)
                windowRect.height += 30f;

            if (WinstonMod.settings.showPawnList && !mcw.nextRaidInfo.sent)
            {
                windowRect.height += 35f;
                windowRect.height += mcw.nextRaidInfo.kindListLines * 16f;
            }
            // Manage width
            windowRect.width = 150f + 10f + ModifierSize + (mcw.nextRaidInfo.modifierCount * ModifierSize);
            // Manage position
            var pos = new Vector2(UI.screenWidth - windowRect.width - mcw.counterPos.x, mcw.counterPos.y);
            windowRect.position = pos;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (WinstonMod.settings.drawBackground)
            {
                GUI.color = WindowBGColor;
                GUI.DrawTexture(inRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }

            if (mcw.nextRaidInfo.sent)
                DoWaveProgressUI(inRect);
            else
                DoWavePredictionUI(inRect);
        }

        private void DoWaveNumberAndModifierUI(Rect rect)
        {
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Modifiers and wave rect
            int i;
            for (i = 1; i <= mcw.nextRaidInfo.modifierCount; i++)
            {
                Rect mRect = new Rect(rect)
                {
                    x = rect.xMax - (i * ModifierSize) - ((i - 1) * 5),
                    width = ModifierSize,
                };

                if (WinstonMod.settings.mysteryMod)
                    ModifierDefOf.VSEWW_Mystery.DrawCard(mRect);
                else
                    mcw.nextRaidInfo.modifiers[i - 1].DrawCard(mRect);
            }

            Rect wRect = new Rect(rect)
            {
                x = rect.xMax - (i * ModifierSize) - ((i - 1) * 5),
                width = ModifierSize,
            };
            GUI.DrawTexture(wRect, Startup.WaveBGTex);
            Widgets.DrawTextureFitted(wRect, mcw.nextRaidInfo.waveType == 0 ? Startup.NormalTex : Startup.BossTex, 0.8f);
            TooltipHandler.TipRegion(wRect, waveTip);
            // Wave number
            Rect waveNumRect = new Rect(rect)
            {
                width = 150f,
            };
            waveNumRect.x = wRect.x - 10 - waveNumRect.width;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(waveNumRect.Rounded(), "VESWW.WaveNum".Translate(mcw.nextRaidInfo.waveNumber));

            Text.Font = prevFont;
            Text.Anchor = prevAnch;
        }

        public void WaveTip()
        {
            string title = mcw.nextRaidInfo.waveType == 0 ? "VESWW.NormalWave".Translate() : "VESWW.BossWave".Translate();
            string pointUsed = "VESWW.PointUsed".Translate(mcw.nextRaidInfo.parms.points);
            string rewardChance = "";

            var c = RewardCommonalities.GetCommonalities(mcw.nextRaidInfo.waveNumber);
            float total = c.Sum(v => v.Value);
            foreach (var item in c)
            {
                rewardChance += $"{item.Key} - {(item.Value > 0 ? item.Value / total : 0).ToStringPercent()}\n";
            }

            waveTip = $"<b>{title}</b>\n\n{pointUsed}\n\n{"VESWW.RewardChance".Translate()}\n{rewardChance}".TrimEndNewlines();
        }

        private void DoWavePredictionUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = ModifierSize
            };
            DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperRight;

            Rect timeRect = new Rect(rect)
            {
                y = numRect.yMax + 10,
                height = 30
            };
            Widgets.Label(timeRect, mcw.nextRaidInfo.TimeBeforeWave());
            Text.Font = GameFont.Tiny;
            float max = timeRect.yMax;
            if (WinstonMod.settings.showPawnList)
            {
                // Faction
                Rect factionIconRect = new Rect(rect)
                {
                    x = numRect.xMax - 20f,
                    y = timeRect.yMax + 10,
                    height = 20f,
                    width = 20f
                };
                GUI.color = mcw.nextRaidInfo.parms.faction.Color;
                GUI.DrawTexture(factionIconRect, mcw.nextRaidInfo.parms.faction.def.FactionIcon);
                GUI.color = Color.white;
                Rect factionRect = new Rect(rect)
                {
                    y = timeRect.yMax + 10,
                    height = 20f,
                    width = rect.width - factionIconRect.width
                };
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(factionRect, mcw.nextRaidInfo.parms.faction.Name);
                Text.Anchor = TextAnchor.UpperRight;
                // Kinds
                Rect kindRect = new Rect(rect)
                {
                    y = factionRect.yMax + 5f,
                    height = mcw.nextRaidInfo.kindListLines * 16f
                };
                Widgets.Label(kindRect, mcw.nextRaidInfo.kindList);
                max = kindRect.yMax;
            }
            // Skip wave button
            Rect skipRect = new Rect(rect)
            {
                y = max + 10,
                height = 20f
            };
            if (Widgets.ButtonText(skipRect, "VESWW.SkipWave".Translate()))
            {
                mcw.StartRaid(Find.TickManager.TicksGame);
            }
            TooltipHandler.TipRegion(skipRect, "VESWW.MoreRewardChance".Translate(mcw.FourthRewardChance(true).ToStringPercent()));
            if (!WinstonMod.settings.hideToggleDraggable)
            {
                // lock button
                Rect lockRect = new Rect(rect)
                {
                    y = skipRect.yMax,
                    height = 25
                };
                Widgets.CheckboxLabeled(lockRect, "VESWW.Locked".Translate(), ref draggable);
            }
            // Restore anchor and font size
            Text.Font = prevFont;
            Text.Anchor = prevAnch;
        }

        private void DoWaveProgressUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 50
            };
            DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            Rect barRect = new Rect(rect)
            {
                y = numRect.yMax + 10,
                height = 25
            };

            int pKill = mcw.nextRaidInfo.totalPawnsBefore - mcw.nextRaidInfo.totalPawnsLeft;
            DrawFillableBar(barRect, $"{pKill}/{mcw.nextRaidInfo.totalPawnsBefore}", (float)pKill / mcw.nextRaidInfo.totalPawnsBefore);
            // Faction
            Rect factionIconRect = new Rect(rect)
            {
                x = numRect.xMax - 20f,
                y = barRect.yMax + 10,
                height = 20f,
                width = 20f
            };
            GUI.color = mcw.nextRaidInfo.parms.faction.Color;
            GUI.DrawTexture(factionIconRect, mcw.nextRaidInfo.parms.faction.def.FactionIcon);
            GUI.color = Color.white;
            Rect factionRect = new Rect(rect)
            {
                y = barRect.yMax + 10,
                height = 20f,
                width = rect.width - factionIconRect.width
            };
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(factionRect, mcw.nextRaidInfo.parms.faction.Name);
            Text.Anchor = TextAnchor.UpperRight;
            // Pawn left
            Text.Anchor = TextAnchor.UpperRight;
            Text.Font = GameFont.Tiny;
            Rect kindRect = new Rect(rect)
            {
                y = factionRect.yMax + 10,
                height = rect.height - numRect.height - barRect.height - 20,
            };
            // - Showing label
            Widgets.Label(kindRect, mcw.nextRaidInfo.cacheKindList);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private void DrawFillableBar(Rect rect, string label, float percent, bool doBorder = true)
        {
            if (doBorder)
            {
                GUI.DrawTexture(rect, BaseContent.BlackTex);
                rect = rect.ContractedBy(3f);
            }
            GUI.color = Widgets.WindowBGFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            Rect fillRect = new Rect(rect);
            fillRect.width *= percent;
            GUI.color = new Color(0.48f, 0.24f, 0.24f);
            GUI.DrawTexture(fillRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            var prevAnch = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = prevAnch;
        }
    }
}