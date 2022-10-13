using System.Collections.Generic;
using Verse;

namespace VSEWW
{
    public class WinstonSettings : ModSettings
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
        public bool hideToggleDraggable = false;
        public bool useRimworldTime = false;
        public bool showPawnList = true;
        public bool dropSlagChunk = true;

        public List<string> modifierDefs = new List<string>();
        public List<string> excludedFactionDefs = new List<string>();
        public List<string> excludedStrategyDefs = new List<string>();

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
            Scribe_Values.Look(ref hideToggleDraggable, "hideToggleDraggable", false);
            Scribe_Values.Look(ref useRimworldTime, "useRimworldTime", false);
            Scribe_Values.Look(ref showPawnList, "showPawnList", true);
            Scribe_Values.Look(ref dropSlagChunk, "dropSlagChunk", true);
            Scribe_Collections.Look(ref modifierDefs, "modifierDefs", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref excludedFactionDefs, "excludedFactionDefs", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref excludedStrategyDefs, "excludedStrategyDefs", LookMode.Value, new List<string>());
        }
    }
}