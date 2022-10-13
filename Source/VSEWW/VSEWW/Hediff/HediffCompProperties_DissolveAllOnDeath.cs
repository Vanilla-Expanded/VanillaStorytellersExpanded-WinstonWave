using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VSEWW
{
    public class HediffCompProperties_DissolveAllOnDeath : HediffCompProperties
    {
        public FleckDef fleck;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public ThingDef filth;
        public int filthCount = 4;
        public SoundDef sound;

        public HediffCompProperties_DissolveAllOnDeath() => compClass = typeof(HediffComp_DissolveAllOnDeath);
    }

    public class HediffComp_DissolveAllOnDeath : HediffComp
    {
        public HediffCompProperties_DissolveAllOnDeath Props => (HediffCompProperties_DissolveAllOnDeath)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % 100 == 0 && (Pawn.IsSlave || Pawn.IsPrisoner))
                Pawn.Kill(new DamageInfo(DamageDefOf.Stab, 9999, instigator: Pawn));
        }

        public override void Notify_PawnKilled()
        {
            if (!Pawn.Spawned)
                return;
            if (Props.fleck != null)
            {
                Vector3 drawPos = Pawn.DrawPos;
                for (int index = 0; index < Props.moteCount; ++index)
                {
                    Vector2 vector2 = Rand.InsideUnitCircle * Props.moteOffsetRange.RandomInRange * Rand.Sign;
                    Vector3 loc = new Vector3(drawPos.x + vector2.x, drawPos.y, drawPos.z + vector2.y);
                    FleckMaker.Static(loc, Pawn.Map, Props.fleck);
                }
            }
            if (Props.filth != null)
                FilthMaker.TryMakeFilth(Pawn.Position, Pawn.Map, Props.filth, Props.filthCount);
            if (Props.sound == null)
                return;
            Props.sound.PlayOneShot(SoundInfo.InMap(Pawn));
            Pawn.equipment.DestroyAllEquipment();
            Pawn.apparel.DestroyAll();

            var toRemove = Pawn.health.hediffSet.hediffs.FindAll(h => !h.def.tendable).ToList();
            for (int i = 0; i < toRemove.Count; i++)
            {
                Pawn.health.RemoveHediff(toRemove[i]);
            }
        }
    }
}