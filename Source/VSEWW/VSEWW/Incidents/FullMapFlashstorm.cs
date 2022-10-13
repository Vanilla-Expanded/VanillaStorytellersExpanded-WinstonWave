using RimWorld;
using Verse;

namespace VSEWW
{
    public class FullMapFlashstorm : GameCondition_Flashstorm
    {
        public override void Init()
        {
            base.Init();
            SingleMap.weatherDecider.DisableRainFor(Duration);
        }

        public override void End()
        {
            base.End();
            Startup.weatherDecider_ticksWhenRainAllowedAgain.SetValue(SingleMap.weatherDecider, Find.TickManager.TicksGame);
        }
    }
}