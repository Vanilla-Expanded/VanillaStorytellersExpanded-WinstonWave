using RimWorld;
using System.Reflection;
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
            var field = SingleMap.weatherDecider.GetType().GetField("ticksWhenRainAllowedAgain", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(SingleMap.weatherDecider, Find.TickManager.TicksGame);
        }
    }
}
