using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public class OrbitalBombardement : GameCondition
    {
        IntVec3 aroundThis = new IntVec3();

        public override bool AllowEnjoyableOutsideNow(Map map) => false;

        public override void Init()
        {
            aroundThis = this.SingleMap.Center;
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
            Map map = this.SingleMap;

            // Explosion handle
            if (!this.nextExplosionCell.IsValid)
            {
                this.ticksToNextEffect = this.bombIntervalTicks;
                this.GetNextExplosionCell();
            }
            this.ticksToNextEffect--;
            if (this.ticksToNextEffect <= 0 && base.TicksLeft >= this.bombIntervalTicks)
            {
                SoundDefOf.Bombardment_PreImpact.PlayOneShot(new TargetInfo(this.nextExplosionCell, map, false));
                this.projectiles.Add(new Bombardment.BombardmentProjectile(200, this.nextExplosionCell));
                this.ticksToNextEffect = this.bombIntervalTicks;
                this.GetNextExplosionCell();
            }
            for (int i = this.projectiles.Count - 1; i >= 0; i--)
            {
                this.projectiles[i].Tick();
                this.Draw();
                if (this.projectiles[i].LifeTime <= 0)
                {
                    IntVec3 targetCell = this.projectiles[i].targetCell;
                    DamageDef bomb = Rand.Range(1, 10) > 2 ? DamageDefOf.Bomb : DamageDefOf.Flame;
                    GenExplosion.DoExplosion(targetCell, map, Rand.Range(3f, 6f), bomb, null);
                    this.projectiles.RemoveAt(i);
                }
            }
        }

        private void Draw()
        {
            if (this.projectiles.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < this.projectiles.Count; i++)
            {
                this.projectiles[i].Draw(ProjectileMaterial);
            }
        }

        private void GetNextExplosionCell()
        {
            this.nextExplosionCell = (from x in GenRadial.RadialCellsAround(aroundThis, 80, true)
                                      where x.InBounds(SingleMap) && !x.Fogged(SingleMap) && !x.Roofed(SingleMap)
                                      select x).RandomElementByWeight((IntVec3 x) => Bombardment.DistanceChanceFactor.Evaluate(x.DistanceTo(aroundThis)));
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
