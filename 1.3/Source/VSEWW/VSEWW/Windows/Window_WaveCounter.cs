using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class Window_WaveCounter : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 300f);

        private readonly MapComponent_Winston mcw;
        private string waveTip;

        public Window_WaveCounter(MapComponent_Winston mapComponent_Winston)
        {
            this.mcw = mapComponent_Winston;
            this.forcePause = false;
            this.absorbInputAroundWindow = false;
            this.closeOnCancel = false;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.draggable = true;
            this.drawShadow = false;
            this.preventCameraMotion = false;
            this.resizeable = false;
            this.doWindowBackground = false;
            this.layer = WindowLayer.GameUI;

            WaveTip();
        }

        public override void WindowOnGUI()
        {
            if (WorldRendererUtility.WorldRenderedNow)
                return;
            base.WindowOnGUI();
        }

        public override void PostClose()
        {
            base.PostClose();
            mcw.waveCounter = null;
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = (float)(UI.screenWidth - windowRect.width - 5f);
            windowRect.y = 5f;
        }

        public void UpdateHeight()
        {
            windowRect.height = 35f + 180f + mcw.nextRaidInfo.kindListLines * 16f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (VESWWMod.settings.drawBackground)
            {
                Color c = new Color
                {
                    r = Widgets.WindowBGFillColor.r,
                    g = Widgets.WindowBGFillColor.g,
                    b = Widgets.WindowBGFillColor.b,
                    a = 0.25f
                };
                Widgets.DrawBoxSolid(inRect.ExpandedBy(5), c);
            }

            if (mcw.nextRaidInfo.sent)
                DoWaveProgressUI(inRect);
            else
                DoWavePredictionUI(inRect);
        }

        private float DoWaveNumberAndModifierUI(Rect rect)
        {
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Modifiers and wave rect
            float mWidth = rect.height - 10;
            int i;
            for (i = 1; i <= mcw.nextRaidInfo.ModifierCount; i++)
            {
                Rect mRect = new Rect(rect)
                {
                    x = rect.xMax - (i * mWidth) - ((i - 1) * 5),
                    width = mWidth,
                    height = mWidth,
                };
                mRect.y += 5;
                GUI.DrawTexture(mRect, Textures.ModifierBGTex);
                mcw.nextRaidInfo.modifiers[i - 1].DrawCard(mRect);
            }

            Rect wRect = new Rect(rect)
            {
                x = rect.xMax - (i * mWidth) - ((i - 1) * 5) - 10,
                width = mWidth + 10,
            };
            GUI.DrawTexture(wRect, Textures.WaveBGTex);
            Widgets.DrawTextureFitted(wRect, mcw.nextRaidInfo.WaveType == 0 ? Textures.NormalTex : Textures.BossTex, 0.8f);
            TooltipHandler.TipRegion(wRect, waveTip);
            // Wave number
            Rect waveNumRect = new Rect(rect)
            {
                width = 150f,
            };
            waveNumRect.x = wRect.x - 10 - waveNumRect.width;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(waveNumRect.Rounded(), "VESWW.WaveNum".Translate(mcw.nextRaidInfo.waveNum));

            Text.Font = prevFont;
            Text.Anchor = prevAnch;

            return waveNumRect.x;
        }

        public void WaveTip()
        {
            string title = mcw.nextRaidInfo.WaveType == 0 ? "VESWW.NormalWave".Translate() : "VESWW.BossWave".Translate();
            string pointUsed = "VESWW.PointUsed".Translate(mcw.nextRaidInfo.incidentParms.points);
            string rewardChance = "";

            var c = RewardCategoryExtension.GetCommonality(mcw.nextRaidInfo.waveNum);
            int total = c.Sum(v => v.Value);
            foreach (var item in c)
            {
                rewardChance += $"{item.Key} - {((float)(item.Value > 0 ? item.Value / (float)total : 0)).ToStringPercent()}\n";
            }

            waveTip = $"<b>{title}</b>\n\n{pointUsed}\n\n{"VESWW.RewardChance".Translate()}\n{rewardChance}".TrimEndNewlines();
        }

        private void DoWavePredictionUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 60
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
                height = 25
            };
            Widgets.Label(timeRect, mcw.nextRaidInfo.TimeBeforeWave());
            Text.Font = GameFont.Tiny;
            // Faction
            Rect factionIconRect = new Rect(rect)
            {
                x = numRect.xMax - 20f,
                y = timeRect.yMax + 10,
                height = 20f,
                width = 20f
            };
            GUI.color = mcw.nextRaidInfo.incidentParms.faction.Color;
            GUI.DrawTexture(factionIconRect, mcw.nextRaidInfo.incidentParms.faction.def.FactionIcon);
            GUI.color = Color.white;
            Rect factionRect = new Rect(rect)
            {
                y = timeRect.yMax + 10,
                height = 20f,
                width = rect.width - factionIconRect.width
            };
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(factionRect, mcw.nextRaidInfo.incidentParms.faction.Name);
            Text.Anchor = TextAnchor.UpperRight;
            // Kinds
            Rect kindRect = new Rect(rect)
            {
                y = factionRect.yMax + 5f,
                height = mcw.nextRaidInfo.kindListLines * 16f
            };
            Widgets.Label(kindRect, mcw.nextRaidInfo.kindList);
            // Skip wave button
            Rect skipRect = new Rect(rect)
            {
                y = kindRect.yMax + 10,
                x = rect.x + ((rect.width / 3) * 2),
                width = rect.width / 3,
                height = 20f
            };
            if (Widgets.ButtonText(skipRect, "VESWW.SkipWave".Translate()))
            {
                mcw.ExecuteRaid(Find.TickManager.TicksGame);
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
                height = 60
            };
            float startAt = DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            Rect barRect = new Rect(rect)
            {
                x = startAt,
                y = numRect.yMax + 10,
                width = rect.width - startAt,
                height = 25
            };

            if (mcw.nextRaidInfo.Lords != null)
            {
                int pKill = mcw.nextRaidInfo.totalPawn - mcw.nextRaidInfo.WavePawnsLeft();
                DrawFillableBar(barRect, $"{pKill}/{mcw.nextRaidInfo.totalPawn}", (float)pKill / mcw.nextRaidInfo.totalPawn);
                // Faction
                Rect factionIconRect = new Rect(rect)
                {
                    x = numRect.xMax - 20f,
                    y = barRect.yMax + 10,
                    height = 20f,
                    width = 20f
                };
                GUI.color = mcw.nextRaidInfo.incidentParms.faction.Color;
                GUI.DrawTexture(factionIconRect, mcw.nextRaidInfo.incidentParms.faction.def.FactionIcon);
                GUI.color = Color.white;
                Rect factionRect = new Rect(rect)
                {
                    y = barRect.yMax + 10,
                    height = 20f,
                    width = rect.width - factionIconRect.width
                };
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(factionRect, mcw.nextRaidInfo.incidentParms.faction.Name);
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
            }
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
