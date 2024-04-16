using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VSEWW
{
    internal class NextRaidInfo : IExposable
    {
        private Map map;

        public int waveType;
        public int waveNumber;

        public bool sent = false;
        public int sentAt = 0;

        public List<Pawn> raidPawns;
        public HashSet<Pawn> outPawns;
        public int totalPawnsBefore;
        public int totalPawnsLeft;

        public IncidentParms parms;

        public int atTick;
        public int generatedAt;

        public string kindList;
        public int kindListLines;
        public string cacheKindList;

        public int modifierCount;
        public bool reinforcementSent = false;
        public bool reinforcementPlanned = false;
        public bool modifiersPreventFlee = false;
        public List<ModifierDef> modifiers = new List<ModifierDef>();
        public List<ModifierDef> mysteryModifiers = new List<ModifierDef>();

        public bool Reinforcements => !reinforcementSent && reinforcementPlanned;

        /// <summary>
        /// Initialize a wave
        /// </summary>
        internal void Init(int wave, float points, Map map)
        {
            this.map = map;

            var ticks = Find.TickManager.TicksGame;
            // Number of day(s) before next wave
            var days = wave > 1 ? WinstonMod.settings.timeBetweenWaves : WinstonMod.settings.timeBeforeFirstWave;
            // Set needed values
            parms = new IncidentParms()
            {
                target = map,
                points = points,
                faction = RandomEnnemyFaction(points),
                raidStrategy = null,
                canKidnap=false,
                canSteal=false
            };
            atTick = ticks + (int)(days * GenDate.TicksPerDay);
            generatedAt = ticks;
            waveNumber = wave;
            waveType = wave % 5 == 0 ? 1 : 0;

            ChooseRandomStrategyDef();
            ChooseModifiers();
            ApplyPrePawnGen();
            SetPawnsInfo();
            ApplyPostPawnGen();
        }

        /// <summary>
        /// Find random faction for the wave
        /// </summary>
        private Faction RandomEnnemyFaction(float points)
        {
            var allFactions = Find.FactionManager.AllFactions;
            var factions = new List<Faction>();

            foreach (var f in allFactions)
            {
                if ((WinstonMod.settings.excludedFactionDefs == null || !WinstonMod.settings.excludedFactionDefs.Contains(f.def.defName))
                    && !f.temporary
                    && !f.defeated
                    && f.HostileTo(Faction.OfPlayer)
                    && f.def.pawnGroupMakers != null
                    && f.def.pawnGroupMakers.Any(p => p.kindDef == PawnGroupKindDefOf.Combat && points <= p.maxTotalPoints)
                    && points > f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
                {
                    factions.Add(f);
                }
            }

            if (factions.Count == 0)
            {
                Find.Storyteller.difficultyDef = InternalDefOf.Peaceful;
                Log.Error($"[VSEWW] No ennemy faction has been found. Switching to Peaceful to prevent further errors.");
                return null;
            }

            factions.TryRandomElementByWeight(f =>
            {
                float num = 1f;
                if (map.StoryState != null && map.StoryState.lastRaidFaction != null && f == map.StoryState.lastRaidFaction)
                {
                    num = 0.4f;
                }
                return f.def.RaidCommonalityFromPoints(points) * num;
            }, out Faction faction);

            return faction;
        }

        /// <summary>
        /// Find strategy for the wave
        /// </summary>
        private void ChooseRandomStrategyDef()
        {
            if (waveType == 1)
            {
                var list = Startup.allOtherStrategies.FindAll(s => CanUseStrategy(s));
                if (list.Count > 0)
                    list.TryRandomElementByWeight(d => d.Worker.SelectionWeightForFaction(map, parms.faction, parms.points), out parms.raidStrategy);
            }

            if (parms.raidStrategy == null)
                Startup.normalStrategies.FindAll(s => CanUseStrategy(s)).TryRandomElementByWeight(d => d.Worker.SelectionWeightForFaction(map, parms.faction, parms.points), out parms.raidStrategy);
        }

        /// <summary>
        /// Check if strategy can be used with current parms
        /// </summary>
        private bool CanUseStrategy(RaidStrategyDef def)
        {
            var excluded = WinstonMod.settings.excludedStrategyDefs;
            if (excluded != null && excluded.Contains(def.defName))
                return false;

            if (def == null || !def.Worker.CanUseWith(parms, PawnGroupKindDefOf.Combat))
                return false;

            return def.arriveModes != null && def.arriveModes.Any(x => x.Worker.CanUseWith(parms));
        }

        /// <summary>
        /// Set pawns prediction string and count
        /// </summary>
        internal void SetPawnsInfo()
        {
            outPawns = new HashSet<Pawn>();
            if (raidPawns.NullOrEmpty())
            {
                // Generate pawns group maker
                var group = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms);
                if (group == null)
                {
                    Log.Error($"[VSEWW] SetPawnsInfo: error generating group for {parms}");
                    return;
                }
                // Generate pawns
                raidPawns = PawnGroupMakerUtility.GeneratePawns(group).ToList();
                if (raidPawns.NullOrEmpty())
                {
                    Log.Error($"[VESWW] SetPawnsInfo: No pawns from parms {parms}");
                    return;
                }
            }

            totalPawnsBefore = totalPawnsLeft = raidPawns.Count;
            // Get all kinds and the number of them
            var kindsCount = new Dictionary<PawnKindDef, int>();
            for (int i = 0; i < raidPawns.Count; i++)
            {
                var kind = raidPawns[i].kindDef;
                if (kindsCount.ContainsKey(kind)) kindsCount[kind]++;
                else kindsCount.Add(kind, 1);
            }
            // Create kinds list string
            string kindLabel = "VESWW.EnemiesC".Translate(totalPawnsBefore) + "\n";
            foreach (var pair in kindsCount)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
            }

            kindList = kindLabel.TrimEndNewlines();
            kindListLines = kindsCount.Count + 1;
        }

        /// <summary>
        /// Choose and add modifier(s)
        /// </summary>
        private void ChooseModifiers()
        {
            var usableModifiers = GetUsableModifiers();
            // Choose two (max) modifiers
            if (usableModifiers.Count > 0)
            {
                int[] modifierChance = GetModifiersChance();

                if (modifierChance[0] > 0 && modifierChance[0] < Rand.Range(0, 100))
                {
                    var modifier = usableModifiers.RandomElement();
                    modifiers.Add(modifier);
                    modifierCount++;
                    usableModifiers.Remove(modifier);
                    usableModifiers.RemoveAll(m => m.incompatibleWith.Contains(modifier));
                }

                if (modifierChance[1] > 0 && modifierChance[1] < Rand.Range(0, 100) && usableModifiers.Count > 0)
                {
                    var modifier = usableModifiers.RandomElement();
                    modifiers.Add(modifier);
                    modifierCount++;
                    usableModifiers.Remove(modifier);
                    usableModifiers.RemoveAll(m => m.incompatibleWith.Contains(modifier));
                }
            }
            // Choose random modifiers if is mystery
            if (usableModifiers.Count > 0)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    if (modifiers[i].mystery)
                    {
                        var modifier = usableModifiers.RandomElement();
                        mysteryModifiers.Add(modifier);
                        usableModifiers.Remove(modifier);
                        usableModifiers.RemoveAll(m => m.incompatibleWith.Contains(modifier));
                    }
                }
            }

            modifiersPreventFlee = modifiers.Any(m => !m.everRetreat) || mysteryModifiers.Any(m => !m.everRetreat);
            reinforcementPlanned = modifiers.Any(m => m.defName == "VSEWW_Reinforcements") || mysteryModifiers.Any(m => m.defName == "VSEWW_Reinforcements");
        }

        /// <summary>
        /// Get all usable modifiers
        /// </summary>
        private List<ModifierDef> GetUsableModifiers()
        {
            var allModifiers = DefDatabase<ModifierDef>.AllDefsListForReading;
            var allUsable = new List<ModifierDef>();

            for (int i = 0; i < allModifiers.Count; i++)
            {
                var m = allModifiers[i];
                if (WinstonMod.settings.modifierDefs.Contains(m.defName))
                    continue;
                if (m.pointMultiplier > 0 && (m.pointMultiplier * parms.points) > WinstonMod.settings.maxPoints)
                    continue;
                if (!parms.faction.def.humanlikeFaction
                    && (!m.allowedWeaponDef.NullOrEmpty()
                    || !m.allowedWeaponCategory.NullOrEmpty()
                    || !m.neededApparelDef.NullOrEmpty()
                    || !m.techHediffs.NullOrEmpty()
                    || !m.globalHediffs.NullOrEmpty()
                    || !m.specificPawnKinds.NullOrEmpty()
                    || !m.everRetreat))
                {
                    continue;
                }

                allUsable.Add(m);
            }

            return allUsable;
        }

        /// <summary>
        /// Get modifiers chance
        /// </summary>
        private int[] GetModifiersChance()
        {
            int modifierChance = 0;

            if (waveType == 1) modifierChance += 10;

            if (waveNumber > 10)
            {
                if (waveNumber <= 15) return new int[] { modifierChance + 3, 0 };
                if (waveNumber <= 20) return new int[] { modifierChance + 10, 0 };
                if (waveNumber <= 25) return new int[] { modifierChance + 20, 0 };
                if (waveNumber <= 30) return new int[] { modifierChance + 25, 0 };
                if (waveNumber <= 35) return new int[] { modifierChance + 28, 0 };
                if (waveNumber <= 40) return new int[] { modifierChance + 30, 0 };
                if (waveNumber <= 45) return new int[] { modifierChance + 35, 0 };
                if (waveNumber <= 50) return new int[] { modifierChance + 35, 5 };
                if (waveNumber <= 60) return new int[] { modifierChance + 50, 10 };
                return new int[] { modifierChance + 80, 20 };
            }
            return new int[] { modifierChance, 0 };
        }

        /// <summary>
        /// Apply modifiers that need to changes things before pawns gen
        /// </summary>
        internal void ApplyPrePawnGen()
        {
            var allModifiers = modifiers;
            allModifiers.AddRange(mysteryModifiers);

            for (int i = 0; i < allModifiers.Count; i++)
            {
                var modifier = allModifiers[i];

                if (modifier.pointMultiplier > 0)
                    parms.points *= modifier.pointMultiplier;

                if (!modifier.everRetreat)
                    parms.canTimeoutOrFlee = false;

                if (!modifier.specificPawnKinds.NullOrEmpty())
                {
                    float point = 0;
                    while (point < parms.points)
                    {
                        var kind = modifier.specificPawnKinds.RandomElement();
                        raidPawns.Add(PawnGenerator.GeneratePawn(kind, parms.faction));
                        point += kind.combatPower;
                    }
                }
            }
        }

        /// <summary>
        /// Apply modifiers that need to changes things after pawns gen
        /// </summary>
        internal void ApplyPostPawnGen()
        {
            var allModifiers = modifiers;
            allModifiers.AddRange(mysteryModifiers);

            for (int i = 0; i < allModifiers.Count; i++)
            {
                var modifier = allModifiers[i];

                for (int p = 0; p < raidPawns.Count; p++)
                {
                    var pawn = raidPawns[p];
                    // Apply global hediff
                    if (!modifier.globalHediffs.NullOrEmpty())
                    {
                        for (int h = 0; h < modifier.globalHediffs.Count; h++)
                            pawn.health.AddHediff(modifier.globalHediffs[h]);
                    }
                    // Apply tech hediff (body parts...)
                    if (!modifier.techHediffs.NullOrEmpty())
                    {
                        for (int h = 0; h < modifier.techHediffs.Count; h++)
                            InstallPart(pawn, modifier.techHediffs[h]);
                    }

                    if (pawn.RaceProps?.intelligence == Intelligence.Humanlike)
                    {
                        if (!modifier.allowedWeaponDef.NullOrEmpty())
                        {
                            // Remove equipements
                            pawn.equipment.DestroyAllEquipment();
                            // Generate new weapon matching defs
                            var newWeaponDef = modifier.allowedWeaponDef.RandomElement();
                            var newWeapon = ThingStuffPair.AllWith(a => a.IsWeapon && a == newWeaponDef).RandomElement();
                            // Add it to the pawn equipement
                            if (newWeapon != null)
                            {
                                var weapon = ThingMaker.MakeThing(newWeapon.thing, newWeapon.stuff);
                                if (weapon.TryGetComp<CompBiocodable>() is CompBiocodable wBioco && wBioco != null)
                                    wBioco.CodeFor(pawn);

                                pawn.equipment.AddEquipment((ThingWithComps)weapon);
                            }
                            // If CE is loaded we regenerate inventory
                            RegenerateInventory(pawn, newWeaponDef);
                        }
                        else if (!modifier.allowedWeaponCategory.NullOrEmpty())
                        {
                            // Remove equipements
                            pawn.equipment.DestroyAllEquipment();
                            // Generate new weapon matching defs
                            var newWeaponDef = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => modifier.allowedWeaponCategory.Any(c => t.IsWithinCategory(c))).RandomElement();
                            var newWeapon = ThingStuffPair.AllWith(a => a.IsWeapon && a == newWeaponDef).RandomElement();
                            // Add it to the pawn equipement
                            if (newWeapon != null)
                            {
                                var weapon = ThingMaker.MakeThing(newWeapon.thing, newWeapon.stuff);
                                if (weapon.TryGetComp<CompBiocodable>() is CompBiocodable wBioco && wBioco != null)
                                    wBioco.CodeFor(pawn);

                                pawn.equipment.AddEquipment((ThingWithComps)weapon);
                            }
                            // If CE is loaded we regenerate inventory
                            RegenerateInventory(pawn, newWeaponDef);
                        }

                        if (!modifier.neededApparelDef.NullOrEmpty())
                        {
                            foreach (var apparelDef in modifier.neededApparelDef)
                            {
                                if (!pawn.apparel.WornApparel.Any(a => a.def == apparelDef))
                                {
                                    ThingStuffPair apparel = ThingStuffPair.AllWith(a => a.IsApparel && a == apparelDef).RandomElement();
                                    if (apparel != null)
                                        pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(apparel.thing, apparel.stuff), false);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Install part on pawn - copy of vanilla private method
        /// </summary>
        private void InstallPart(Pawn pawn, ThingDef partDef)
        {
            IEnumerable<RecipeDef> source = DefDatabase<RecipeDef>.AllDefs.Where(x => x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x));
            if (!source.Any())
                return;
            RecipeDef recipe = source.RandomElement();
            if (!recipe.Worker.GetPartsToApplyOn(pawn, recipe).Any())
                return;
            recipe.Worker.ApplyOnPawn(pawn, recipe.Worker.GetPartsToApplyOn(pawn, recipe).RandomElement(), null, new List<Thing>(), null);
        }

        /// <summary>
        /// CE Regenerate inventory method
        /// </summary>
        internal void RegenerateInventory(Pawn pawn, ThingDef newWeaponDef)
        {
            if (Startup.CEActive)
            {
                pawn.inventory.DestroyAll();
                PawnInventoryGenerator.GenerateInventoryFor(pawn, new PawnGenerationRequest(pawn.kindDef));
                if (newWeaponDef.IsRangedWeapon)
                {
                    // Remove shield(s)
                    var appToRemove = pawn.apparel.WornApparel.FindAll(a => a.def.thingCategories != null && a.def.thingCategories.Any(c => c.defName == "Shields"));
                    for (int i = 0; i < appToRemove.Count; i++)
                    {
                        pawn.apparel.Remove(appToRemove[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Save data
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref sent, "sent", false);
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Values.Look(ref sentAt, "sentAt");
            Scribe_Values.Look(ref reinforcementSent, "reinforcementSent", false);
            Scribe_Values.Look(ref waveNumber, "waveNum");
            Scribe_Values.Look(ref kindList, "kindList");
            Scribe_Values.Look(ref kindListLines, "kindListLines");
            Scribe_Values.Look(ref cacheKindList, "cacheKindList");
            Scribe_Values.Look(ref totalPawnsLeft, "totalPawnsLeft");
            Scribe_Values.Look(ref totalPawnsBefore, "totalPawnsBefore");
            Scribe_Values.Look(ref reinforcementPlanned, "reinforcementPlanned");

            Scribe_Deep.Look(ref parms, "incidentParms");

            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Collections.Look(ref mysteryModifiers, "mysteryModifier", LookMode.Def);
            // If raid sent, lord(s) is up, pawns are saved in it, we only need ref
            Scribe_Collections.Look(ref raidPawns, "raidPawns", sent ? LookMode.Reference : LookMode.Deep);
            Scribe_Collections.Look(ref outPawns, "outPawns", LookMode.Reference);
        }

        /// <summary>
        /// Check if raid is over, update pawns count/kinds
        /// </summary>
        public bool RaidOver()
        {
            // If not sent it's not over
            if (!sent)
                return false;

            var pawnOutCount = 0;
            var pawnsToDefeat = new Dictionary<PawnKindDef, int>();

            for (int i = 0; i < raidPawns.Count; i++)
            {
                var pawn = raidPawns[i];

                // Pawn is out
                if (outPawns.Contains(pawn))
                {
                    pawnOutCount++;
                }
                else if (pawn.Dead || pawn.Downed || pawn.InMentalState)
                {
                    outPawns.Add(pawn);
                    pawnOutCount++;
                }
                // Populate defeat dic
                else
                {
                    if (pawnsToDefeat.ContainsKey(pawn.kindDef))
                        pawnsToDefeat[pawn.kindDef]++;
                    else
                        pawnsToDefeat.Add(pawn.kindDef, 1);
                }
                // Remove transitions to flee toil if any modifier have everRetreat to false
                if (modifiersPreventFlee)
                    pawn?.CurJob?.lord?.Graph?.transitions?.RemoveAll(t => t.target is LordToil_PanicFlee);
            }
            // Update pawn left count
            totalPawnsLeft = totalPawnsBefore - pawnOutCount;
            // Update pawn left report string
            var desc = (string)"VESWW.EnemiesR".Translate() + "\n";
            for (int i = 0; i < pawnsToDefeat.Count; i++)
            {
                var pair = pawnsToDefeat.ElementAt(i);
                desc += $"{pair.Value} {pair.Key.LabelCap}\n";
            }
            cacheKindList = desc.TrimEndNewlines();
            // Handle reinforcement
            if (Reinforcements && totalPawnsLeft <= (int)(totalPawnsLeft * 0.8f))
            {
                reinforcementSent = true;
                // Create parms
                var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, this.parms.target);
                parms.faction = this.parms.faction;
                parms.points = Math.Max(100f, this.parms.points * 0.5f);
                parms.pawnGroupMakerSeed = Rand.RangeInclusive(1, 10000);
                parms.customLetterLabel = "VESWW.Reinforcement".Translate();
                // Execute
                IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
            }

            return totalPawnsLeft == 0;
        }

        /// <summary>
        /// Set pawns prediction string and count
        /// </summary>
        public void SendRaid(Map map, int ticks)
        {
            // Slow down, keep track of raids
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            map.StoryState.lastRaidFaction = parms.faction;
            // Generate raid loot
            GenerateRaidLoot();
            // Resolve stuff and send pawns
            ResolveRaidArrival();
            // Make letter label/text
            var letterLabel = (TaggedString)parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
            var letterText = (TaggedString)GetLetterText();

            var relatedLetterText = "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(raidPawns, ref letterLabel, ref letterText, relatedLetterText, true);
            // Get letter target(s)
            var targetInfoList = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                var source = IncidentParmsUtility.SplitIntoGroups(raidPawns, parms.pawnGroups);
                var list = source.MaxBy(x => x.Count);

                if (list.Any())
                    targetInfoList.Add(list[0]);

                for (int i = 0; i < source.Count; ++i)
                {
                    if (source[i] != list && source[i].Any())
                        targetInfoList.Add(source[i][0]);
                }
            }
            else if (raidPawns.Any())
            {
                for (int i = 0; i < raidPawns.Count; i++)
                {
                    targetInfoList.Add(raidPawns[i]);
                }
            }
            // Send letter
            SendLetter(letterLabel, letterText, targetInfoList);

            if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer)
                parms.raidStrategy.Worker.MakeLords(parms, raidPawns);

            // Manage nextRaidInfo
            SendIncidentModifiers();
            sentAt = ticks;
            sent = true;
        }

        /// <summary>
        /// Generate raiders loot
        /// </summary>
        private void GenerateRaidLoot()
        {
            if (parms.faction.def.raidLootMaker == null || !raidPawns.Any())
                return;

            var raidLootPoints = parms.points * Find.Storyteller.difficulty.EffectiveRaidLootPointsFactor;
            var num = parms.faction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);

            if (parms.raidStrategy != null)
                num *= parms.raidStrategy.raidLootValueFactor;

            List<Thing> loot = parms.faction.def.raidLootMaker.root.Generate(new ThingSetMakerParams()
            {
                totalMarketValueRange = new FloatRange?(new FloatRange(num, num)),
                makingFaction = parms.faction
            });

            new WinstonRaidLootDistributor(raidPawns, loot).DistributeLoot();
        }

        /// <summary>
        /// Resolve raid arrival mode and spawn center
        /// </summary>
        private void ResolveRaidArrival()
        {
            if (parms.raidArrivalMode == null && !parms.raidStrategy.arriveModes.Where(x => x.Worker.CanUseWith(parms)).TryRandomElementByWeight(x => x.Worker.GetSelectionWeight(parms), out parms.raidArrivalMode))
            {
                Log.Error("[VESWW] Could not resolve arrival mode for raid. Defaulting to EdgeWalkIn. parms=" + parms);
                parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            }

            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                Log.Error($"[VESWW] Couldn't reslove raid spawn center. parms=" + parms);
                return;
            }

            parms.raidArrivalMode.Worker.Arrive(raidPawns, parms);
        }

        /// <summary>
        /// Create raid letter text
        /// </summary>
        private string GetLetterText()
        {
            var letterText = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst() + "\n\n" + parms.raidStrategy.arrivalTextEnemy;
            var pawn = raidPawns.Find(x => x.Faction.leader == x);

            if (pawn != null)
                letterText = letterText + "\n\n" + "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER")).Resolve();

            if (parms.raidAgeRestriction != null && !parms.raidAgeRestriction.arrivalTextExtra.NullOrEmpty())
                letterText = letterText + "\n\n" + parms.raidAgeRestriction.arrivalTextExtra.Formatted(parms.faction.def.pawnsPlural.Named("PAWNSPLURAL")).Resolve();

            return letterText;
        }

        /// <summary>
        /// Send raid letter
        /// </summary>
        private void SendLetter(TaggedString label, TaggedString text, LookTargets lookTargets)
        {
            var letter = LetterMaker.MakeLetter(label, text, LetterDefOf.ThreatBig, lookTargets, parms.faction, parms.quest, parms.letterHyperlinkThingDefs);
            Find.LetterStack.ReceiveLetter(letter);
        }

        /// <summary>
        /// Send incidents modifiers
        /// </summary>
        public void SendIncidentModifiers()
        {
            foreach (var modifier in modifiers)
            {
                if (!modifier.incidents.NullOrEmpty())
                {
                    modifier.incidents.ForEach(i =>
                    {
                        Find.Storyteller.incidentQueue.Add(i, Find.TickManager.TicksGame, new IncidentParms()
                        {
                            target = map
                        });
                    });
                }
            }
        }

        /// <summary>
        /// Stop incidents modifiers
        /// </summary>
        public void StopIncidentModifiers()
        {
            var incidents = new List<IncidentDef>();
            for (int m = 0; m < modifierCount; m++)
            {
                if (modifiers[m].incidents is List<IncidentDef> _incidents)
                    incidents.AddRange(_incidents);
            }

            for (int i = 0; i < incidents.Count; i++)
            {
                var incident = incidents[i];
                var conditions = map.GameConditionManager.ActiveConditions;
                for (int c = 0; c < conditions.Count; c++)
                {
                    var condition = conditions[c];
                    if (incident.gameCondition == condition.def)
                        condition.End();
                }
            }
        }

        /// <summary>
        /// Get time before this raid (IRL or rimworld)
        /// </summary>
        public string TimeBeforeWave() => (atTick - Find.TickManager.TicksGame).ToStringTicksToPeriod();

        public override string ToString()
        {
            return $"sent:{sent} atTick:{atTick} parms:{parms}";
        }
    }
}