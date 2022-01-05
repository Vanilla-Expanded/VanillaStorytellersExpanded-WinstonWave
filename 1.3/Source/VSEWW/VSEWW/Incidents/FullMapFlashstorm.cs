using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    public class FullMapFlashstorm : GameCondition_Flashstorm
    {

        public override void Init()
        {
            base.Init();
            this.SingleMap.weatherDecider.DisableRainFor(this.Duration);
        }

        public override void End()
        {
            base.End();
            var field = SingleMap.weatherDecider.GetType().GetField("ticksWhenRainAllowedAgain", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(SingleMap.weatherDecider, Find.TickManager.TicksGame);
        }
    }
}
