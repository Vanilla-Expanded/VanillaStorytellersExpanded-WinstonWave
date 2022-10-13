using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public class OrbitalBombardement : GameCondition
    {
        private IntVec3 aroundThis = new IntVec3();

        public override bool AllowEnjoyableOutsideNow(Map map) => false;

        public override void Init()
        {
            aroundThis = SingleMap.Center;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref aroundThis, "aroundThis");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!nextExplosionCell.IsValid)
                    GetNextExplosionCell();
                if (projectiles == null)
                    projectiles = new List<Bombardment.BombardmentProjectile>();
            }
        }

        private readonly int bombIntervalTicks = 200;
        private int ticksToNextEffect;
        private IntVec3 nextExplosionCell = new IntVec3();
        private List<Bombardment.BombardmentProjectile> projectiles = new List<Bombardment.BombardmentProjectile>();

        public override void GameConditionTick()
        {
            Map map = SingleMap;

            // Explosion handle
            if (!nextExplosionCell.IsValid)
            {
                ticksToNextEffect = bombIntervalTicks;
                GetNextExplosionCell();
            }
            ticksToNextEffect--;
            if (ticksToNextEffect <= 0 && base.TicksLeft >= bombIntervalTicks)
            {
                SoundDefOf.Bombardment_PreImpact.PlayOneShot(new TargetInfo(nextExplosionCell, map, false));
                projectiles.Add(new Bombardment.BombardmentProjectile(200, nextExplosionCell));
                ticksToNextEffect = bombIntervalTicks;
                GetNextExplosionCell();
            }
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Tick();
                Draw();
                if (projectiles[i].LifeTime <= 0)
                {
                    IntVec3 targetCell = projectiles[i].targetCell;
                    DamageDef bomb = Rand.Range(1, 10) > 2 ? DamageDefOf.Bomb : DamageDefOf.Flame;
                    GenExplosion.DoExplosion(targetCell, map, Rand.Range(3f, 6f), bomb, null);
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void Draw()
        {
            if (projectiles.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(ProjectileMaterial);
            }
        }

        private void GetNextExplosionCell()
        {
            nextExplosionCell = CellFinderLoose.RandomCellWith(x => x.InBounds(SingleMap) && !x.Fogged(SingleMap) && !x.Roofed(SingleMap), SingleMap);
        }

        public static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
        {
            {
                new CurvePoint(0f, 1f),
                true
            },
            {
                new CurvePoint(15f, 0.1f),
                true
            }
        };

        private static readonly Material ProjectileMaterial = MaterialPool.MatFrom("Things/Projectile/Bullet_Big", ShaderDatabase.Transparent, Color.white);
    }
}