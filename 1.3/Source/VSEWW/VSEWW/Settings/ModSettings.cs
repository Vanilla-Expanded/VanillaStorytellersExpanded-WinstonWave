using System.Collections.Generic;
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

        public List<string> modifierDefs = new List<string>();

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
            Scribe_Collections.Look(ref modifierDefs, "modifierDefs", LookMode.Value, new List<string>());
        }
    }

    class VESWWMod : Mod
    {
        private string _timeBeforeFirstWave;
        private string _timeBetweenWaves;
        private string _maxPoints;
        private string _pointMultiplierBefore;
        private string _pointMultiplierAfter;

        private Vector2 scrollPosition = Vector2.zero;
        private float settingsHeight = 0f;

        public float SettingsHeight
        {
            get
            {
                if (settingsHeight == 0f)
                {
                    settingsHeight = (8 * 12f) + ((13 + DefDatabase<ModifierDef>.DefCount) * 32f);
                }
                return settingsHeight;
            }
        }

        public static VESWWModSettings settings;

        public VESWWMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<VESWWModSettings>();
        }

        public override string SettingsCategory() => "VESWW.ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect)
            {
                height = SettingsHeight,
                width = inRect.width - 20
            };

            Widgets.BeginScrollView(inRect, ref scrollPosition, rect);
            Listing_Standard lst = new Listing_Standard();
            lst.Begin(rect);

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
            lst.Gap();

            lst.CheckboxLabeled("VESWW.DrawBack".Translate(), ref settings.drawBackground);
            lst.Gap();

            lst.CheckboxLabeled("VESWW.EnableStats".Translate(), ref settings.enableStatIncrease);
            lst.Gap();

            if (settings.modifierDefs != null)
            {
                foreach (var modifier in DefDatabase<ModifierDef>.AllDefsListForReading)
                {
                    bool enabled = !settings.modifierDefs.Contains(modifier.defName);
                    if (lst.ButtonText($"{modifier.LabelCap} (Enabled: {enabled})", modifier.description))
                    {
                        if (enabled)
                            settings.modifierDefs.Add(modifier.defName);
                        else
                            settings.modifierDefs.Remove(modifier.defName);
                    }
                }
            }
            else
                settings.modifierDefs = new List<string>();

            lst.End();
            Widgets.EndScrollView();
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
