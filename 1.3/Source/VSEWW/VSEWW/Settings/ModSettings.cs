using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class VESWWModSettings : ModSettings
    {
        public int timeBeforeFirstWave = 5;
        public int timeBetweenWaves = 2;
        public int maxPoints = 25000;
        public bool drawCounterBackground = false;
        public bool compactCounter = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.timeBeforeFirstWave, "timeBeforeFirstWave", 5);
            Scribe_Values.Look(ref this.timeBetweenWaves, "timeBetweenWaves", 2);
            Scribe_Values.Look(ref this.maxPoints, "maxPoints", 25000);
            Scribe_Values.Look(ref this.drawCounterBackground, "drawCounterBackground", false);
            Scribe_Values.Look(ref this.compactCounter, "compactCounter", false);
        }
    }

    class VESWWMod : Mod
    {
        private string _timeBeforeFirstWave;
        private string _timeBetweenWaves;
        private string _maxPoints;

        public static VESWWModSettings settings;

        public VESWWMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<VESWWModSettings>();
        }

        public override string SettingsCategory() => "VESWW.ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard lst = new Listing_Standard();
            lst.Begin(inRect);

            lst.Label("VESWW.TimeBeforeFirstWave".Translate());
            lst.IntEntry(ref settings.timeBeforeFirstWave, ref _timeBeforeFirstWave);

            lst.Label("VESWW.TimeBetweenWaves".Translate());
            lst.IntEntry(ref settings.timeBetweenWaves, ref _timeBetweenWaves);

            lst.Label("VESWW.MaxPoints".Translate());
            lst.IntEntry(ref settings.maxPoints, ref _maxPoints, 100);

            lst.GapLine();
            lst.Label("VESWW.MaxPoints".Translate());

            lst.CheckboxLabeled("VESWW.DrawBackground".Translate(), ref settings.drawCounterBackground);
            lst.CheckboxLabeled("VESWW.CompactCounter".Translate(), ref settings.compactCounter);

            lst.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            var w = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            if (w != null && w.waveCounter != null)
                w.waveCounter.UpdateHeightAndWidth();
        }
    }
}
