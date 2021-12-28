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
        public override Vector2 InitialSize => new Vector2(300f, 300f);

        private readonly MapComponent_Winston mcw;

        public Window_WaveCounter(MapComponent_Winston mapComponent_Winston)
        {
            this.mcw = mapComponent_Winston;
            this.forcePause = false;
            this.absorbInputAroundWindow = false;
            this.closeOnCancel = false;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.draggable = true;
            this.drawShadow = false;
            this.preventCameraMotion = false;
            this.onlyOneOfTypeAllowed = false;
            this.resizeable = false;
            this.layer = WindowLayer.GameUI;
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            this.windowRect.x = (float)(UI.screenWidth - this.InitialSize.x - 50.0);
            this.windowRect.y = 50f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard lst = new Listing_Standard();
            lst.Begin(inRect);

            if (mcw.nextRaidInfo.sent)
                DoWaveProgressUI(lst);
            else
                DoWavePredictionUI(lst);

            lst.End();
        }

        private void DoWavePredictionUI(Listing_Standard lst)
        {
            lst.Label($"Wave {mcw.nextRaidInfo.waveNum}:");
            lst.Label($"Faction: {mcw.nextRaidInfo.incidentParms.faction.NameColored}:");
            lst.Label($"In: {mcw.nextRaidInfo.TimeBeforeWave()}");
            lst.Label($"Enemies comming:");
            foreach (var pair in mcw.nextRaidInfo.pawnKinds)
            {
                lst.Label($"{pair.Value} {pair.Key.LabelCap}");
            }
        }

        private void DoWaveProgressUI(Listing_Standard lst)
        {
            lst.Label($"Wave {mcw.nextRaidInfo.waveNum}");
            lst.Label($"Faction: {mcw.nextRaidInfo.incidentParms.faction.NameColored}:");
            lst.Label($"{mcw.nextRaidInfo.totalPawn - mcw.nextRaidInfo.WavePawnsLeft()} / {mcw.nextRaidInfo.totalPawn}");
            lst.Label($"Defeat remaining enemies:");

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
                lst.Label($"{pair.Value} {pair.Key.LabelCap}");
            }
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
    }
}
