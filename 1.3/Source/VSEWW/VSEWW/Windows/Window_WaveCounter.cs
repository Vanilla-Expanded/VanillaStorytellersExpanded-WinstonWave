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
        public override Vector2 InitialSize => new Vector2(500f, 300f);

        private readonly MapComponent_Winston mcw;

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
            this.windowRect.x = (float)(UI.screenWidth - this.InitialSize.x - 50.0);
            this.windowRect.y = 50f;
        }

        public override void DoWindowContents(Rect inRect)
        {
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

            // Wave number
            Rect waveNumRect = new Rect(rect)
            {
                width = rect.width / 3,
            };
            Widgets.Label(waveNumRect.Rounded(), "VESWW.WaveNum".Translate(mcw.nextRaidInfo.waveNum));

            // Modifiers rect
            Rect modifierRect = new Rect(rect)
            {
                width = (rect.width / 3) * 2,
                x = waveNumRect.xMax
            };
            float mWidth = Math.Min(modifierRect.width / 3, modifierRect.height);
            for (int i = 1; i <= 3; i++)
            {
                Rect mRect = new Rect(modifierRect)
                {
                    x = modifierRect.xMax - (i * mWidth) - ((i - 1) * 5),
                    width = mWidth
                };
                Widgets.DrawWindowBackground(mRect);
            }

            Text.Font = prevFont;
            Text.Anchor = prevAnch;
        }

        private void DoWavePredictionUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 50
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
            Text.Font = prevFont;
            // Kinds
            Rect kindRect = new Rect(rect)
            {
                y = timeRect.yMax + 10,
                height = rect.height - numRect.height - timeRect.height - 20,
            };
            string kindLabel = "VESWW.EnemiesC".Translate() + "\n";
            foreach (var pair in mcw.nextRaidInfo.pawnKinds)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
            }
            Widgets.Label(kindRect, kindLabel.TrimEndNewlines());
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

            int pKill = mcw.nextRaidInfo.totalPawn - mcw.nextRaidInfo.WavePawnsLeft();
            DrawFillableBar(barRect, $"{pKill}/{mcw.nextRaidInfo.totalPawn}", (float)pKill / mcw.nextRaidInfo.totalPawn);
            // Pawn left
            var prevAnch = Text.Anchor;
            Text.Anchor = TextAnchor.UpperRight;
            Rect kindRect = new Rect(rect)
            {
                y = barRect.yMax + 10,
                height = rect.height - numRect.height - barRect.height - 20,
            };
            // - Building label
            string kindLabel = "VESWW.EnemiesR".Translate() + "\n";
            Dictionary<PawnKindDef, int> toDefeat = new Dictionary<PawnKindDef, int>();
            mcw.nextRaidInfo.WavePawns().ForEach(p =>
            {
                if (toDefeat.ContainsKey(p.kindDef))
                    toDefeat[p.kindDef]++;
                else
                    toDefeat.Add(p.kindDef, 1);
            });
            foreach (var pair in toDefeat)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
            }
            // - Showing label
            Widgets.Label(kindRect, kindLabel.TrimEndNewlines());
            Text.Anchor = prevAnch;
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
