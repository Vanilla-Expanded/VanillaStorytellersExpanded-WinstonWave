﻿using System;
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
            if (reward.sendRewardOf > RewardCategory.Poor)
            {
                SendReward(DefDatabase<RewardDef>.AllDefsListForReading.FindAll(r => r.category == reward.sendRewardOf).RandomElement(), map);
            }
            else
            {
                if (reward.incidentDef != null)
                {
                    Find.Storyteller.incidentQueue.Add(reward.incidentDef, Find.TickManager.TicksGame, new IncidentParms
                    {
                        target = map
                    });
                }
                if (reward.massHeal)
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
                if (reward.unlockXResearch > 0)
                {
                    for (int i = 0; i < reward.unlockXResearch; i++)
                    {
                        var r = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find(x => x.CanStartNow);
                        if (r != null)
                        {
                            Find.ResearchManager.FinishProject(r);
                            Messages.Message("VESWW.ResearchUnlocked".Translate(r.LabelCap), MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
                if (reward.boostSkillBy > 0)
                {
                    map.mapPawns.AllPawns.FindAll(p => p.Faction == Faction.OfPlayer && p.RaceProps.intelligence == Intelligence.Humanlike).ForEach(p =>
                    {
                        var pSkills = DefDatabase<SkillDef>.AllDefs.Where(x => !p.skills.GetSkill(x).TotallyDisabled);
                        foreach (var s in pSkills)
                        {
                            p.skills.GetSkill(s).levelInt += Math.Min(reward.boostSkillBy, 20 - p.skills.GetSkill(s).levelInt);
                        }
                    });
                }

                var winston = map.GetComponent<MapComponent_Winston>();
                if (winston != null && winston.nextRaidInfo != null && reward.waveModifier != null)
                {
                    if (reward.waveModifier.delayBy > 0)
                        winston.nextRaidInfo.atTick += reward.waveModifier.delayBy * 60000;
                    if (reward.waveModifier.weakenBy > 0)
                        winston.nextRaidMultiplyPoints = reward.waveModifier.weakenBy;
                    if (reward.waveModifier.allies)
                        winston.nextRaidSendAllies = true;
                }

                List<Thing> thingList = new List<Thing>();

                if (!reward.randomItems.NullOrEmpty()) thingList.AddRange(GenerateRandomItems(reward));
                if (!reward.randomPawns.NullOrEmpty()) thingList.AddRange(GenerateRandomPawns(reward));
                if (!reward.items.NullOrEmpty()) thingList.AddRange(GenerateItems(reward));
                if (!reward.pawns.NullOrEmpty()) thingList.AddRange(GeneratePawns(reward));

                if (thingList.Count > 0)
                {
                    IntVec3 intVec3 = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.DropPodIncoming.Size, map.ParentFaction);
                    DropPodUtility.DropThingsNear(intVec3, map, thingList, leaveSlag: true, canRoofPunch:false);
                }
            }
        }

        private static List<Thing> GenerateItems(RewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var i in reward.items)
            {
                int countLeft = i.count;
                while (countLeft > 0)
                {
                    Thing thing;
                    if (i.thing.CostStuffCount > 0)
                        thing = ThingMaker.MakeThing(i.thing, GenStuff.RandomStuffFor(i.thing));
                    else
                        thing = ThingMaker.MakeThing(i.thing);
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

        private static List<Thing> GeneratePawns(RewardDef reward)
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

        private static List<Thing> GenerateRandomItems(RewardDef reward)
        {
            List<Thing> things = new List<Thing>();

            foreach (var i in reward.randomItems)
            {
                var chooseFrom = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => 
                    i.thingCategories.Any(c => t.IsWithinCategory(c)) &&
                    !i.excludeThingCategories.Any(c => t.IsWithinCategory(c)) &&
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

        private static List<Thing> GenerateRandomPawns(RewardDef reward)
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
