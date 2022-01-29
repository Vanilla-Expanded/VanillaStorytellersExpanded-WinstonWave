using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class VESWWModSettings : ModSettings
    {
        public float timeBeforeFirstWave = 5f;
        public float timeBetweenWaves = 1.2f;
        public int maxPoints = 25000;
        public float pointMultiplierBefore = 1.2f;
        public float pointMultiplierAfter = 1.1f;
        public bool enableStatIncrease = true;
        public bool drawBackground = false;
        public bool mysteryMod = false;
        public bool randomRewardMod = false;

        public List<string> modifierDefs = new List<string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeBeforeFirstWave, "timeBeforeFirstWave", 5f);
            Scribe_Values.Look(ref timeBetweenWaves, "timeBetweenWaves", 1.2f);
            Scribe_Values.Look(ref maxPoints, "maxPoints", 25000);
            Scribe_Values.Look(ref pointMultiplierBefore, "pointMultiplierBefore", 1.2f);
            Scribe_Values.Look(ref pointMultiplierAfter, "pointMultiplierAfter", 1.1f);
            Scribe_Values.Look(ref enableStatIncrease, "enableStatIncrease", true);
            Scribe_Values.Look(ref drawBackground, "drawBackground", false);
            Scribe_Values.Look(ref mysteryMod, "mysteryMod", false);
            Scribe_Values.Look(ref randomRewardMod, "randomRewardMod", false);
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

            lst.CheckboxLabeled("VESWW.MysteryMod".Translate(), ref settings.mysteryMod, "VESWW.MysteryModTip".Translate());
            lst.Gap();

            lst.CheckboxLabeled("VESWW.RandomRewardMod".Translate(), ref settings.randomRewardMod, "VESWW.RandomRewardModTip".Translate());
            lst.GapLine();

            lst.Label("VESWW.TimeBeforeFirstWave".Translate(), tooltip: "VESWW.TimeBeforeFirstWaveTip".Translate());
            lst.TextFieldNumeric(ref settings.timeBeforeFirstWave, ref _timeBeforeFirstWave, 1f, 10f);
            lst.Gap();

            lst.Label("VESWW.TimeBetweenWaves".Translate(), tooltip: "VESWW.TimeBetweenWavesTip".Translate());
            lst.TextFieldNumeric(ref settings.timeBetweenWaves, ref _timeBetweenWaves, 1f, 10f);
            lst.GapLine();

            lst.Label("VESWW.MaxPoints".Translate(), tooltip: "VESWW.MaxPointsTip".Translate());
            lst.IntEntry(ref settings.maxPoints, ref _maxPoints, 10);
            lst.Gap();

            lst.Label("VESWW.PointMultiplierBefore20".Translate(), tooltip: "VESWW.PointMultiplierBefore20Tip".Translate());
            lst.TextFieldNumeric(ref settings.pointMultiplierBefore, ref _pointMultiplierBefore, 1f, 10f);
            lst.Gap();

            lst.Label("VESWW.PointMultiplierAfter20".Translate(), tooltip: "VESWW.PointMultiplierAfter20Tip".Translate());
            lst.TextFieldNumeric(ref settings.pointMultiplierAfter, ref _pointMultiplierAfter, 1f, 10f);
            lst.GapLine();

            lst.CheckboxLabeled("VESWW.EnableStats".Translate(), ref settings.enableStatIncrease, "VESWW.EnableStatsTip".Translate());
            lst.Gap();

            lst.CheckboxLabeled("VESWW.DrawBack".Translate(), ref settings.drawBackground);
            lst.GapLine();

            lst.Label("VESWW.Modifiers".Translate());
            lst.Gap();

            if (settings.modifierDefs != null)
            {
                foreach (var modifier in DefDatabase<ModifierDef>.AllDefsListForReading)
                {
                    bool enabled = !settings.modifierDefs.Contains(modifier.defName);
                    if (lst.ButtonTextLabeled($"{modifier.LabelCap}", $"{enabled}"))
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
