using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VSEWW
{
    public class StorytellerCompProperties_ThreatCycle : StorytellerCompProperties
    {
        public StorytellerCompProperties_ThreatCycle()
        {
            this.compClass = typeof(StorytellerComp_ThreatCycle);
        }

        public int daysBeforeFirstWave = 3;
        public int daysBetweenWaves = 5;
        public int threatPointIncreasePerWave = 5;
    }

    public class StorytellerComp_ThreatCycle : StorytellerComp
    {
        
    }
}
