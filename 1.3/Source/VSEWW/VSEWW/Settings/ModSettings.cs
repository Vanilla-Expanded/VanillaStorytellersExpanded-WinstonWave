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
        public int timeBetweenWaves = 1;
        public int maxPoints = 25000;
        public float pointMultiplierBefore = 1.2f;
        public float pointMultiplierAfter = 1.1f;
        public bool enableStatIncrease = true;
        public bool drawBackground = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeBeforeFirstWave, "timeBeforeFirstWave", 5);
            Scribe_Values.Look(ref timeBetweenWaves, "timeBetweenWaves", 1);
            Scribe_Values.Look(ref maxPoints, "maxPoints", 25000);
            Scribe_Values.Look(ref pointMultiplierBefore, "pointMultiplierBefore", 1.2f);
            Scribe_Values.Look(ref pointMultiplierAfter, "pointMultiplierAfter", 1.1f);
            Scribe_Values.Look(ref enableStatIncrease, "enableStatIncrease", true);
            Scribe_Values.Look(ref drawBackground, "drawBackground", false);
        }
    }

    class VESWWMod : Mod
    {
        private string _timeBeforeFirstWave;
        private string _timeBetweenWaves;
        private string _maxPoints;
        private string _pointMultiplierBefore;
        private string _pointMultiplierAfter;

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
            lst.Gap();

            lst.Label("VESWW.TimeBetweenWaves".Translate());
            lst.IntEntry(ref settings.timeBetweenWaves, ref _timeBetweenWaves);
            lst.Gap();

            lst.Label("VESWW.MaxPoints".Translate());
            lst.IntEntry(ref settings.maxPoints, ref _maxPoints, 10);
            lst.Gap();

            lst.Label("VESWW.PointMultiplierBefore20".Translate());
            lst.TextFieldNumeric(ref settings.pointMultiplierBefore, ref _pointMultiplierBefore, 1f, 10f);
            lst.Gap();

            lst.Label("VESWW.PointMultiplierAfter20".Translate());
            lst.TextFieldNumeric(ref settings.pointMultiplierAfter, ref _pointMultiplierAfter, 1f, 10f);
            lst.Gap();

            lst.CheckboxLabeled("VESWW.EnableStats".Translate(), ref settings.enableStatIncrease);

            lst.CheckboxLabeled("VESWW.DrawBack".Translate(), ref settings.drawBackground);

            lst.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (settings.enableStatIncrease)
                Find.CurrentMap?.GetComponent<MapComponent_Winston>()?.AddStatHediff();
            else
                Find.CurrentMap?.GetComponent<MapComponent_Winston>()?.RemoveStatHediff();
        }
    }
}
