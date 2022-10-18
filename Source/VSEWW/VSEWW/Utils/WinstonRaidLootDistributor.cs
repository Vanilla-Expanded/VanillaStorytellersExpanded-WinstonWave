using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class WinstonRaidLootDistributor
    {
        private readonly List<Pawn> allPawns;
        private readonly List<Thing> loot;
        private readonly List<Pawn> unusedPawns;
        private Pawn recipient;
        private float recipientMassGiven;

        public WinstonRaidLootDistributor(List<Pawn> allPawns, List<Thing> loot)
        {
            this.allPawns = allPawns;
            this.loot = loot;
            unusedPawns = new List<Pawn>(allPawns.Where(x => !x.RaceProps.Animal));
        }

        public void DistributeLoot()
        {
            recipient = unusedPawns.MaxBy(p => p.kindDef.combatPower);
            recipientMassGiven = 0.0f;
            foreach (var things in loot.GroupBy(t => t.def))
            {
                foreach (Thing thing in things)
                    DistributeItem(thing);
                NextRecipient();
            }
        }

        private void DistributeItem(Thing item)
        {
            int stackCount = item.stackCount;
            int num = 0;
            while (stackCount > 0 && num++ < 5)
            {
                stackCount -= TryGiveToRecipient(item, stackCount);
                if (stackCount > 0)
                    NextRecipient();
            }
            if (stackCount <= 0)
                return;
            NextRecipient();
            TryGiveToRecipient(item, stackCount, true);
        }

        private int TryGiveToRecipient(Thing item, int count, bool force = false)
        {
            float num = 10f * Mathf.Max(1f, this.recipient.BodySize) - recipientMassGiven;
            float statValue = item.GetStatValue(StatDefOf.Mass);
            int count1 = force ? count : Mathf.RoundToInt(Mathf.Clamp(num / statValue, 0.0f, count));
            if (count1 <= 0)
                return 0;
            int recipient = this.recipient.inventory.innerContainer.TryAdd(item, count1, true);
            recipientMassGiven += recipient * statValue;
            return recipient;
        }

        private void NextRecipient()
        {
            recipientMassGiven = 0.0f;
            if (unusedPawns.Any())
                recipient = unusedPawns.Pop();
            else
                recipient = allPawns.RandomElement();
        }
    }
}