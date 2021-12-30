using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VSEWW
{
    public static class RewardCreator
    {
        public static void SendReward(RewardDef reward, Map map)
        {
            if (reward is IncidentRewardDef incidentRewardDef)
            {
                Find.Storyteller.incidentQueue.Add(incidentRewardDef.incidentDef, Find.TickManager.TicksGame, new IncidentParms
                {
                    target = map
                });
            }
            else if (reward is MassHealRewardDef)
            {
                map.mapPawns.AllPawns.FindAll(p => p.Faction == Faction.OfPlayer).ForEach(p =>
                {
                    var tmpHediffs = p.health.hediffSet.hediffs.ToList();
                    for (int i = 0; i < tmpHediffs.Count; i++)
                    {
                        if (tmpHediffs[i] is Hediff_Injury injury)
                            p.health.RemoveHediff(injury);
                        else if (tmpHediffs[i] is Hediff_MissingPart missingPart && missingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && (missingPart.Part.parent == null || p.health.hediffSet.GetNotMissingParts().Contains(missingPart.Part.parent)))
                            p.health.RestorePart(missingPart.Part);
                    }
                });
            }
            else if (reward is RCategoryRewardDef rCategoryRewardDef)
            {
                SendReward(DefDatabase<RewardDef>.AllDefsListForReading.FindAll(r => r.category == rCategoryRewardDef.rewardOf).RandomElement(), map);
            }
            else if (reward is ReseachRewardDef reseachRewardDef)
            {
                for (int i = 0; i < reseachRewardDef.count; i++)
                {
                    var r = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find(x => x.CanStartNow);
                    Find.ResearchManager.FinishProject(r);
                    Messages.Message("VESWW.ResearchUnlocked".Translate(r.LabelCap), MessageTypeDefOf.NeutralEvent);
                }
            }
            else if (reward is SkillBoosterRewardDef skillBoosterRewardDef)
            {
                map.mapPawns.AllPawns.FindAll(p => p.Faction == Faction.OfPlayer && p.RaceProps.intelligence == Intelligence.Humanlike).ForEach(p =>
                {
                    var pSkills = DefDatabase<SkillDef>.AllDefs.Where(x => !p.skills.GetSkill(x).TotallyDisabled);
                    foreach (var s in pSkills)
                    {
                        p.skills.GetSkill(s).levelInt += Math.Min(skillBoosterRewardDef.count, 20 - p.skills.GetSkill(s).levelInt);
                    }
                });
            }
            else if (reward is WaveModifierRewardDef waveModifierRewardDef)
            {
                // TODO Wave modifier rewards
            }
            else
            {
                List<Thing> thingList = CreateThingListFromRewardDef(reward);
                IntVec3 near = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.DropPodIncoming.Size, map.ParentFaction);
                RCellFinder.TryFindRandomCellNearWith(near, i => i.Walkable(map) && !i.Roofed(map), map, out IntVec3 intVec3);

                DropPodUtility.DropThingsNear(intVec3, map, thingList, leaveSlag: true);
            }
        }

        private static List<Thing> CreateThingListFromRewardDef(RewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            if (reward is BlockRewardDef blockRewardDef) things.AddRange(FromBlockRewardDef(blockRewardDef));
            if (reward is RItemRewardDef rItemRewardDef) things.AddRange(FromRItemRewardDef(rItemRewardDef));
            if (reward is RPawnRewardDef rPawnRewardDef) things.AddRange(FromRPawnRewardDef(rPawnRewardDef));
            if (reward is ItemRewardDef itemRewardDef) things.AddRange(FromItemRewardDef(itemRewardDef));
            if (reward is PawnRewardDef pawnRewardDef) things.AddRange(FromPawnRewardDef(pawnRewardDef));

            return things;
        }

        private static List<Thing> FromBlockRewardDef(BlockRewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            ThingDef blockDef = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.thingCategories != null && t.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks)).RandomElement();
            int countLeft = reward.count;
            while (countLeft > 0)
            {
                Thing block = ThingMaker.MakeThing(blockDef);
                int stack = Math.Min(countLeft, blockDef.stackLimit);
                block.stackCount = stack;
                countLeft -= stack;
                things.Add(block);
            }

            return things;
        }

        private static List<Thing> FromItemRewardDef(ItemRewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var i in reward.items)
            {
                int countLeft = i.count;
                while (countLeft > 0)
                {
                    Thing thing = ThingMaker.MakeThing(i.thing);
                    if (thing.TryGetComp<CompQuality>() is CompQuality cq)
                    {
                        cq.SetQuality(i.quality, ArtGenerationContext.Outsider);
                    }

                    int stack = Math.Min(countLeft, thing.def.stackLimit);
                    thing.stackCount = stack;
                    countLeft -= stack;

                    if (thing.def.mineable)
                    {
                        thing.MakeMinified();
                    }

                    things.Add(thing);
                }
            }

            return things;
        }

        private static List<Thing> FromPawnRewardDef(PawnRewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var pr in reward.pawns)
            {
                for (int i = 0; i < pr.count; i++)
                {
                    Pawn p;
                    if (pr.pawnkind.RaceProps.Humanlike)
                    {
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pr.pawnkind, Faction.OfPlayer, mustBeCapableOfViolence: true, fixedIdeo: Faction.OfPlayer.ideos.PrimaryIdeo));
                        p.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    }                        
                    else
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pr.pawnkind, Faction.OfPlayer));

                    things.Add(p);
                }   
            }

            return things;
        }

        private static List<Thing> FromRItemRewardDef(RItemRewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var i in reward.randomItems)
            {
                var chooseFrom = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => 
                    i.thingCategories.Any(c => t.IsWithinCategory(c)) &&
                    t.tradeability != Tradeability.None &&
                    !t.destroyOnDrop && 
                    t.BaseMarketValue > 0);

                int countLeft = i.count;
                while (countLeft > 0)
                {
                    ThingDef thingDef = chooseFrom.RandomElement();
                    Thing thing;
                    if (thingDef.CostStuffCount > 0)
                        thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
                    else
                        thing = ThingMaker.MakeThing(thingDef);

                    if (thing.TryGetComp<CompQuality>() is CompQuality cq)
                    {
                        cq.SetQuality(i.quality, ArtGenerationContext.Outsider);
                    }

                    int stack = Math.Min(countLeft, thing.def.stackLimit);
                    thing.stackCount = stack;
                    countLeft -= stack;

                    if (thing.def.mineable)
                    {
                        thing.MakeMinified();
                    }

                    things.Add(thing);
                }
            }

            return things;
        }

        private static List<Thing> FromRPawnRewardDef(RPawnRewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var pr in reward.randomPawns)
            {
                var pawnChoices = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(p => p.RaceProps.intelligence == pr.intelligence);
                if (pr.tradeTag != "") pawnChoices.RemoveAll(p => p.race.tradeTags != null && !p.race.tradeTags.Contains(pr.tradeTag));
                if (pr.minCombatPower > 0) pawnChoices.RemoveAll(p => p.combatPower >= pr.minCombatPower);
                if (pr.maxCombatPower > 0) pawnChoices.RemoveAll(p => p.combatPower <= pr.maxCombatPower);

                for (int i = 0; i < pr.count; i++)
                {
                    Pawn p;
                    PawnKindDef pawnkind = pawnChoices.RandomElement();
                    if (pawnkind.RaceProps.Humanlike)
                    {
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind, Faction.OfPlayer, mustBeCapableOfViolence: true, fixedIdeo: Faction.OfPlayer.ideos.PrimaryIdeo));
                        p.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    }
                    else
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind, Faction.OfPlayer));

                    things.Add(p);
                }
            }

            return things;
        }
    }
}
