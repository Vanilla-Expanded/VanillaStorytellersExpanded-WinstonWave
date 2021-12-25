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
        public int timeBeforeFirstWave = 3;
        public int timeBetweenWaves = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.timeBeforeFirstWave, "timeBeforeFirstWave");
            Scribe_Values.Look(ref this.timeBetweenWaves, "timeBetweenWaves");
        }
    }

    class VESWWMod : Mod
    {
        private string _timeBeforeFirstWave;
        private string _timeBetweenWaves;

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
        }
    }
}
