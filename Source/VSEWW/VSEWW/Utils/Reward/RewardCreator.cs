using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using static System.Collections.Specialized.BitVector32;

namespace VSEWW
{
    internal static class RewardCreator
    {
        private static void HealEverything(Pawn p)
        {
            if (p.health != null && p.health.hediffSet != null && !p.health.hediffSet.hediffs.NullOrEmpty())
            {
                var tmpHediffs = p.health.hediffSet.hediffs.ToList();
                for (int i = 0; i < tmpHediffs.Count; i++)
                {
                    if (tmpHediffs[i] is Hediff_Injury injury)
                        p.health.RemoveHediff(injury);
                    else if (tmpHediffs[i] is Hediff_MissingPart missingPart && missingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && (missingPart.Part.parent == null || p.health.hediffSet.GetNotMissingParts().Contains(missingPart.Part.parent)))
                        p.health.RestorePart(missingPart.Part);
                }
            }
        }

        private static void HealAllPawns(List<Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
                HealEverything(pawns[i]);
        }

        public static void SendReward(RewardDef reward, Map map, MapComponent_Winston mapComp)
        {
            if (reward.sendRewardOf > RewardCategory.Poor)
            {
                var randomReward = Startup.rewardsPerCat[reward.sendRewardOf].RandomElement();
                Messages.Message("VESWW.RandRewardOutcome".Translate(randomReward.LabelCap), MessageTypeDefOf.NeutralEvent);
                SendReward(randomReward, map, mapComp);
                return;
            }

            if (reward.incidentDef != null)
            {
                Find.Storyteller.incidentQueue.Add(reward.incidentDef, Find.TickManager.TicksGame, new IncidentParms
                {
                    target = map
                });
            }

            if (reward.massHeal)
            {
                HealAllPawns(map.mapPawns.FreeColonistsSpawned);
                HealAllPawns(map.mapPawns.SlavesAndPrisonersOfColonySpawned);
            }

            if (reward.unlockXResearch > 0)
            {
                for (int i = 0; i < reward.unlockXResearch; i++)
                {
                    var projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.FindAll(x => x.CanStartNow);
                    if (!projects.NullOrEmpty())
                    {
                        var rReward = projects.RandomElement();
                        Find.ResearchManager.FinishProject(rReward);
                        Messages.Message("VESWW.ResearchUnlocked".Translate(rReward.LabelCap), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }

            if (reward.boostSkillBy > 0)
            {
                var pawns = map.mapPawns.AllPawns;
                for (int i = 0; i < pawns.Count; i++)
                {
                    var pawn = pawns[i];
                    if (pawn.Faction != Faction.OfPlayer || pawn.RaceProps.intelligence != Intelligence.Humanlike)
                        continue;

                    var skills = pawn.skills.skills;
                    for (int o = 0; o < skills.Count; o++)
                    {
                        var skill = skills[o];
                        if (skill.levelInt < 20)
                            skill.levelInt = Math.Min(skill.levelInt + reward.boostSkillBy, 20);
                    }
                }
            }

            if (mapComp != null && mapComp.nextRaidInfo != null && reward.waveModifier != null)
            {
                if (reward.waveModifier.weakenBy > 0)
                    mapComp.nextRaidMultiplyPoints = reward.waveModifier.weakenBy;
                if (reward.waveModifier.allies)
                    mapComp.nextRaidSendAllies = true;

                mapComp.nextRaidInfo.atTick += reward.waveModifier.delayBy * 60000;
            }

            var thingsToSend = new List<Thing>();

            GenerateRandomItems(reward, ref thingsToSend);
            GenerateRandomPawns(reward, ref thingsToSend);
            GenerateItems(reward, ref thingsToSend);
            GeneratePawns(reward, ref thingsToSend);

            if (thingsToSend.Count > 0)
            {
                if (map == null)
                    map = Find.CurrentMap;

                IntVec3 intVec3 = mapComp.dropSpot != IntVec3.Invalid ? mapComp.dropSpot : DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.DropPodIncoming.Size, map.ParentFaction);
                DropPodUtility.DropThingsNear(intVec3, map, thingsToSend, leaveSlag: WinstonMod.settings.dropSlagChunk, canRoofPunch: false, forbid: false);
            }
        }

        private static void GenerateItems(RewardDef reward, ref List<Thing> thingsToSend)
        {
            if (reward.items.NullOrEmpty())
                return;

            for (int i = 0; i < reward.items.Count; i++)
            {
                var itemReward = reward.items[i];
                var countLeft = itemReward.count;
                var thingDef = itemReward.thing;

                while (countLeft > 0)
                {
                    Thing thing;
                    if (thingDef.CostStuffCount > 0)
                        thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
                    else
                        thing = ThingMaker.MakeThing(thingDef);

                    if (thing.TryGetComp<CompQuality>() is CompQuality comp)
                        comp.SetQuality(itemReward.quality > QualityCategory.Awful ? itemReward.quality : QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);

                    int stack = Math.Min(countLeft, thingDef.stackLimit);
                    thing.stackCount = stack;
                    countLeft -= stack;

                    if (thing.def.minifiedDef != null)
                        thing = thing.MakeMinified();

                    thingsToSend.Add(thing);
                }
            }
        }

        private static void GeneratePawns(RewardDef reward, ref List<Thing> thingsToSend)
        {
            if (reward.pawns.NullOrEmpty())
                return;

            for (int i1 = 0; i1 < reward.pawns.Count; i1++)
            {
                var pawn = reward.pawns[i1];
                for (int i = 0; i < pawn.count; i++)
                {
                    Pawn p;
                    if (pawn.pawnkind.RaceProps.Humanlike)
                    {
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.pawnkind, Faction.OfPlayer, mustBeCapableOfViolence: true, fixedIdeo: Faction.OfPlayer.ideos.PrimaryIdeo));
                        p.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    }
                    else
                    {
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.pawnkind, Faction.OfPlayer));
                    }

                    thingsToSend.Add(p);
                }
            }
        }

        private static void GenerateRandomItems(RewardDef reward, ref List<Thing> thingsToSend)
        {
            if (reward.randomItems.NullOrEmpty())
                return;

            for (int i = 0; i < reward.randomItems.Count; i++)
            {
                var item = reward.randomItems[i];
                var chooseFrom = item.randomFrom.NullOrEmpty() ? DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t =>
                    item.thingCategories.Any(c => t.IsWithinCategory(c)) && t.tradeability != Tradeability.None && !t.destroyOnDrop && t.BaseMarketValue > 0) : item.randomFrom;

                if (!item.excludeThingCategories.NullOrEmpty())
                    chooseFrom.RemoveAll(t => item.excludeThingCategories.Any(c => t.IsWithinCategory(c)));

                int countLeft = item.count;
                while (countLeft > 0)
                {
                    ThingDef thingDef = chooseFrom.RandomElement();

                    Thing thing;
                    if (thingDef.CostStuffCount > 0)
                        thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
                    else
                        thing = ThingMaker.MakeThing(thingDef);

                    if (thing.TryGetComp<CompQuality>() is CompQuality comp)
                        comp.SetQuality(item.quality > QualityCategory.Awful ? item.quality : QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);

                    // If it's not a body part
                    if (!thingDef.isTechHediff)
                    {
                        int stack = Math.Min(countLeft, thing.def.stackLimit);
                        thing.stackCount = stack;
                        countLeft -= stack;
                    }
                    else
                    {
                        countLeft--;
                    }

                    if (thing.def.minifiedDef != null)
                        thing = thing.MakeMinified();

                    thingsToSend.Add(thing);
                }
            }
        }

        private static void GenerateRandomPawns(RewardDef reward, ref List<Thing> thingsToSend)
        {
            if (reward.randomPawns.NullOrEmpty())
                return;

            for (int i1 = 0; i1 < reward.randomPawns.Count; i1++)
            {
                RPawnReward pr = reward.randomPawns[i1];
                var pawnChoices = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(p => p.RaceProps.intelligence == pr.intelligence && p.race.tradeTags?.Contains("NonContractable")!=true);
                if (pr.tradeTag != "") pawnChoices.RemoveAll(p => p.race.tradeTags?.Contains(pr.tradeTag) != true);
                if (pr.excludeInsectoid) pawnChoices.RemoveAll(p => p.RaceProps.Insect);
                

                bool skipMin = false;
                if (pr.minCombatPower > 0 && pr.maxCombatPower > 0)
                    skipMin = !pawnChoices.Any(p => p.combatPower >= pr.minCombatPower && p.combatPower <= pr.maxCombatPower);

                if (skipMin && pr.minCombatPower > 0) pawnChoices.RemoveAll(p => p.combatPower < pr.minCombatPower);
                if (pr.maxCombatPower > 0) pawnChoices.RemoveAll(p => p.combatPower > pr.maxCombatPower);

                pawnChoices.RemoveAll(p => p.RaceProps.IsAnomalyEntity || p.defaultFactionType == FactionDefOf.Entities || p.race == InternalDefOf.CreepJoiner);


                for (int i = 0; i < pr.count; i++)
                {
                    Pawn p;
                    PawnKindDef pawnkind = pawnChoices.RandomElement();
                    if (pawnkind.RaceProps.Humanlike)
                    {
                        bool dontGiveWeapon = pr.ghoul || pr.shambler;
                        bool forbidAnyTitle = pr.shambler;

                        PawnGenerationRequest request = new PawnGenerationRequest(pawnkind, Faction.OfPlayer, mustBeCapableOfViolence: true, fixedIdeo: Faction.OfPlayer.ideos.PrimaryIdeo, dontGiveWeapon: dontGiveWeapon, forbidAnyTitle: forbidAnyTitle);
                        request.IsCreepJoiner = false;
                     
                        p = PawnGenerator.GeneratePawn(request);

                        if (pr.ghoul) { MutantUtility.SetPawnAsMutantInstantly(p, MutantDefOf.Ghoul); }

                        p.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    }
                    else
                    {
                        p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind, Faction.OfPlayer));
                    }

                    if (pr.shambler) { 
                        MutantUtility.SetPawnAsMutantInstantly(p, MutantDefOf.Shambler);
                        p.SetFaction(Faction.OfPlayer);
                    }

                    thingsToSend.Add(p);
                }
            }
        }
    }
}