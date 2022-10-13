using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VSEWW
{
    internal class WorldComponent_KillCounter : WorldComponent
    {
        private int totalKill = 0;

        public WorldComponent_KillCounter(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalKill, "totalKill", 0, true);
        }

        public void AddKills(int n)
        {
            Log.Message($"Adding {n} kill(s) to counter");
            totalKill += n;
        }

        public int GetTotal()
        {
            Map map = Find.CurrentMap;
            if (map != null && map.mapPawns.AnyColonistSpawned)
            {
                map.mapPawns.AllPawnsSpawned.FindAll(p => p.IsColonist).ForEach(p =>
                {
                    var n = (int)(p.records.GetValue(RecordDefOf.KillsHumanlikes) + p.records.GetValue(RecordDefOf.KillsMechanoids));
                    totalKill += n;
                    Log.Message($"Adding {n} kill(s) to counter");
                });
            }

            var wP = Find.World.worldPawns.GetPawnsBySituation(WorldPawnSituation.Kidnapped).ToList().FindAll(p => p.IsColonist);
            if (!wP.NullOrEmpty())
            {
                wP.ForEach(p =>
                 {
                     var n = (int)(p.records.GetValue(RecordDefOf.KillsHumanlikes) + p.records.GetValue(RecordDefOf.KillsMechanoids));
                     totalKill += n;
                     Log.Message($"Adding {n} kill(s) to counter");
                 });
            }

            return totalKill;
        }
    }
}