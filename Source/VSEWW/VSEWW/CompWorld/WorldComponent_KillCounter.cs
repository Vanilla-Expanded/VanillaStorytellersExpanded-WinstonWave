using RimWorld.Planet;
using Verse;

namespace VSEWW
{
    internal class WorldComponent_KillCounter : WorldComponent
    {
        public int totalKill = 0;

        public WorldComponent_KillCounter(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalKill, "totalKill", 0, true);
        }

        public void IncrementCounter() => totalKill++;
    }
}