using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VSEWW
{
    internal class NextRaidInfo : IExposable
    {
        public bool sent = false;
        public Map map;

        private List<Lord> lords;

        public int atTick;
        public int generatedAt;
        public int sentAt = 0;

        public List<ModifierDef> modifiers = new List<ModifierDef>();
        public bool modifiersPreventFlee = false;
        public bool reinforcementSent = false;
        public int modifierCount;

        public List<Pawn> raidPawns = new List<Pawn>();
        public IncidentParms incidentParms;
        public int reinforcementSeed = -1;

        public int waveNum;
        public int waveType;

        public int kindListLines;
        public string kindList;

        private List<Pawn> lordPawnsCache = new List<Pawn>();
        internal string cacheKindList;
        private int cacheTick = 0;
        public int totalPawns;

        public List<ModifierDef> mysteryModifier;

        public List<Lord> Lords
        {
            get
            {
                if (!lords.NullOrEmpty())
                    return lords;

                lords = map.lordManager?.lords?.FindAll(l => l.faction == incidentParms.faction && l.AnyActivePawn);

                return lords;
            }
        }

        public bool Reinforcements => !reinforcementSent && modifiers.Any(m => m.defName == "VSEWW_Reinforcements");

        public float FourthRewardChance
        {
            get
            {
                float ticksInAdvance = atTick - sentAt;
                float ticksInBetween = atTick - generatedAt;
                if (ticksInAdvance > 0) // Sent early
                {
                    return ticksInAdvance / ticksInBetween;
                }
                return 0f;
            }
        }

        public float FourthRewardChanceNow
        {
            get
            {
                float ticksInAdvance = atTick - Find.TickManager.TicksGame;
                float ticksInBetween = atTick - generatedAt;
                if (ticksInAdvance > 0) // Sent early
                {
                    return ticksInAdvance / ticksInBetween;
                }
                return 0f;
            }
        }

        public bool RaidOver
        {
            get
            {
                return sent && Lords != null && WavePawnsLeft() == 0 && map.mapPawns.AnyColonistSpawned;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref sent, "sent");
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Values.Look(ref sentAt, "sentAt");
            Scribe_Values.Look(ref reinforcementSent, "reinforcementSent", false);
            Scribe_Values.Look(ref waveNum, "waveNum");
            Scribe_Values.Look(ref kindList, "kindList");
            Scribe_Values.Look(ref kindListLines, "kindListLines");
            Scribe_Values.Look(ref cacheTick, "cacheTick");
            Scribe_Values.Look(ref cacheKindList, "cacheKindList");
            Scribe_Values.Look(ref totalPawns, "totalPawn");
            Scribe_Values.Look(ref totalPawns, "totalPawn");
            Scribe_Values.Look(ref reinforcementSeed, "reinforcementSeed", -1);

            Scribe_Deep.Look(ref incidentParms, "incidentParms");

            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Collections.Look(ref lords, "lords", LookMode.Reference);
            Scribe_Collections.Look(ref mysteryModifier, "mysteryModifier", LookMode.Def);

            if (lords.NullOrEmpty()) // Pawns are not part of a lord yet, else we don't save them, they are saved in the lord(s)
                Scribe_Collections.Look(ref raidPawns, "raidPawns", LookMode.Deep); // Deep save them
        }

        /// <summary>
        /// Get time before this raid (IRL or rimworld)
        /// </summary>
        public string TimeBeforeWave()
        {
            if (WinstonMod.settings.useRimworldTime)
            {
                return (atTick - Find.TickManager.TicksGame).ToStringTicksToPeriod();
            }
            else
            {
                return TimeSpanExtension.Verbose(TimeSpan.FromSeconds((atTick - Find.TickManager.TicksGame).TicksToSeconds()));
            }
        }

        /// <summary>
        /// Get all pawns part of the raid - with caching
        /// </summary>
        public List<Pawn> WavePawns()
        {
            if (lordPawnsCache.NullOrEmpty() || cacheTick % 600 == 0)
            {
                string desc = "VESWW.EnemiesR".Translate() + "\n";

                lordPawnsCache = new List<Pawn>();
                // Get all pawns in lord(s) & pawns kinds
                var lords = Lords;
                var toDefeat = new Dictionary<PawnKindDef, int>();

                for (int l = 0; l < lords.Count; l++)
                {
                    var lord = lords[l];
                    if (lord != null && lord.AnyActivePawn)
                    {
                        // Foreach pawn in lord
                        for (int p = 0; p < lord.ownedPawns.Count; p++)
                        {
                            var pawn = lord.ownedPawns[p];
                            // If not downed and not in mental state
                            if (pawn != null && !pawn.Downed && !pawn.mindState.mentalStateHandler.InMentalState)
                            {
                                // Count it
                                lordPawnsCache.Add(pawn);
                                // Get it's kind
                                if (toDefeat.ContainsKey(pawn.kindDef))
                                    toDefeat[pawn.kindDef]++;
                                else
                                    toDefeat.Add(pawn.kindDef, 1);
                            }
                        }
                    }
                }

                // Add kinds and kinds count to string
                for (int i = 0; i < toDefeat.Count; i++)
                {
                    var pair = toDefeat.ElementAt(i);
                    desc += $"{pair.Value} {pair.Key.LabelCap}\n";
                }
                cacheKindList = desc.TrimEndNewlines();
                // Recount
                totalPawns = lordPawnsCache.Count;
                // Send reinforcement if needed
                if (Reinforcements && totalPawns <= (int)(totalPawns * 0.8f))
                {
                    reinforcementSent = true;
                    ++Find.StoryWatcher.statsRecord.numRaidsEnemy;
                    // Create parms
                    var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, incidentParms.target);
                    parms.faction = incidentParms.faction;
                    parms.points = Math.Max(100f, incidentParms.points * 0.5f);
                    parms.pawnGroupMakerSeed = Rand.RangeInclusive(1, 10000);
                    parms.customLetterLabel = "VESWW.Reinforcement".Translate();
                    // Set seed
                    reinforcementSeed = parms.pawnGroupMakerSeed.Value;
                    // Execute
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                }
                // Remove flee toil if any modifier have everRetreat to false
                if (modifiersPreventFlee)
                {
                    for (int l = 0; l < lords.Count; l++)
                    {
                        lords[l].Graph.transitions.RemoveAll(t => t.target is LordToil_PanicFlee);
                    }
                }
            }

            cacheTick++;
            return lordPawnsCache;
        }

        /// <summary>
        /// Get pawns count left
        /// </summary>
        public int WavePawnsLeft() => WavePawns().Count;

        /// <summary>
        /// Get modifiers chance
        /// </summary>
        private int[] GetModifiersChance()
        {
            int modifierChance = 0;

            if (waveType == 1) modifierChance += 10;

            if (waveNum > 10)
            {
                if (waveNum <= 15) return new int[] { modifierChance + 3, 0 };
                if (waveNum <= 20) return new int[] { modifierChance + 10, 0 };
                if (waveNum <= 25) return new int[] { modifierChance + 20, 0 };
                if (waveNum <= 30) return new int[] { modifierChance + 25, 0 };
                if (waveNum <= 35) return new int[] { modifierChance + 28, 0 };
                if (waveNum <= 40) return new int[] { modifierChance + 30, 0 };
                if (waveNum <= 45) return new int[] { modifierChance + 35, 0 };
                if (waveNum <= 50) return new int[] { modifierChance + 35, 5 };
                if (waveNum <= 60) return new int[] { modifierChance + 50, 10 };
                return new int[] { modifierChance + 80, 20 };
            }
            return new int[] { modifierChance, 0 };
        }

        /// <summary>
        /// Choose and add modifier(s)
        /// </summary>
        public void ChooseAndApplyModifier()
        {
            var modifiersPool = GetModifiersPool();

            if (modifiersPool.Count > 0)
            {
                int[] modifiersChance = GetModifiersChance();

                var rand = new Random();
                if (modifiersChance[0] > 0 && modifiersChance[0] < rand.Next(0, 100))
                {
                    var modi = modifiersPool.RandomElement();
                    modifiers.Add(modi);
                    modifierCount++;
                    modifiersPool.Remove(modi);
                    modifiersPool.RemoveAll(m => m.incompatibleWith.Contains(modi));
                }

                if (modifiersChance[1] > 0 && modifiersChance[1] < rand.Next(0, 100) && modifiersPool.Count > 0)
                {
                    modifiers.Add(modifiersPool.RandomElement());
                    modifierCount++;
                }

                ApplyModifiers(true);
            }

            modifiersPreventFlee = modifiers.Any(m => !m.everRetreat);
        }

        /// <summary>
        /// Get all usable modifiers
        /// </summary>
        public List<ModifierDef> GetModifiersPool()
        {
            var modifiersPool = DefDatabase<ModifierDef>.AllDefsListForReading.FindAll(m => !WinstonMod.settings.modifierDefs.Contains(m.defName));
            modifiersPool.RemoveAll(m => m.pointMultiplier > 0 && (m.pointMultiplier * incidentParms.points) > WinstonMod.settings.maxPoints);

            if (!incidentParms.faction.def.humanlikeFaction)
            {
                modifiersPool.RemoveAll(m => !m.allowedWeaponDef.NullOrEmpty() ||
                                             !m.allowedWeaponCategory.NullOrEmpty() ||
                                             !m.neededApparelDef.NullOrEmpty() ||
                                             !m.techHediffs.NullOrEmpty() ||
                                             !m.globalHediffs.NullOrEmpty() ||
                                             !m.specificPawnKinds.NullOrEmpty() ||
                                             !m.everRetreat);
            }

            return modifiersPool;
        }

        /// <summary>
        /// Apply all modifiers. Call ApplyModifier
        /// </summary>
        public void ApplyModifiers(bool first = false)
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.mystery)
                {
                    if (mysteryModifier.NullOrEmpty())
                    {
                        mysteryModifier = new List<ModifierDef>();
                        var modifiersPool = GetModifiersPool();
                        var modi = modifiers.Find(m => m != modifier);
                        modifiersPool.Remove(modi);
                        modifiersPool.RemoveAll(m => m.incompatibleWith.Contains(modi));

                        if (!modifiersPool.NullOrEmpty())
                            mysteryModifier.Add(modifiersPool.RandomElement());
                    }

                    ApplyModifier(mysteryModifier.First(), first);
                }
                else
                {
                    ApplyModifier(modifier, first);
                }
            }
        }

        /// <summary>
        /// Apply single modifier
        /// </summary>
        public void ApplyModifier(ModifierDef modifier, bool first = false)
        {
            if (first)
            {
                if (modifier.pointMultiplier > 0) // Can only be applied before raid is sent
                    incidentParms.points *= modifier.pointMultiplier;

                if (!modifier.everRetreat)
                    incidentParms.canTimeoutOrFlee = false;

                if (!modifier.specificPawnKinds.NullOrEmpty())
                {
                    float point = 0;
                    while (point < incidentParms.points)
                    {
                        var kind = modifier.specificPawnKinds.RandomElement();
                        raidPawns.Add(PawnGenerator.GeneratePawn(kind, incidentParms.faction));
                        point += kind.combatPower;
                    }
                }
            }

            if (!raidPawns.NullOrEmpty() && !first) // Pawn modifier, only applied if pawns are generated
            {
                foreach (var pawn in raidPawns)
                {
                    if (!modifier.everRetreat)
                    {
                        pawn.mindState.canFleeIndividual = false;
                    }

                    if (!modifier.globalHediffs.NullOrEmpty())
                    {
                        foreach (var hediff in modifier.globalHediffs)
                        {
                            pawn.health.AddHediff(hediff);
                        }
                    }

                    if (!modifier.techHediffs.NullOrEmpty())
                    {
                        foreach (var hediff in modifier.techHediffs)
                        {
                            InstallPart(pawn, hediff);
                        }
                    }

                    if (pawn.RaceProps?.intelligence == Intelligence.Humanlike)
                    {
                        if (!modifier.allowedWeaponDef.NullOrEmpty())
                        {
                            // Remove equipements
                            pawn.equipment.DestroyAllEquipment();
                            // Generate new weapon matching defs
                            var newWeaponDef = modifier.allowedWeaponDef.RandomElement();
                            ThingStuffPair newWeapon = ThingStuffPair.AllWith(a => a.IsWeapon && a == newWeaponDef).RandomElement();
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
                            ThingStuffPair newWeapon = ThingStuffPair.AllWith(a => a.IsWeapon && a == newWeaponDef).RandomElement();
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
        /// Set pawns prediction string and count
        /// </summary>
        public void SetPawnsInfo()
        {
            if (raidPawns.NullOrEmpty())
                raidPawns = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms)).ToList();
            // Get all kinds and the number of them
            var tempDic = new Dictionary<PawnKindDef, int>();
            foreach (Pawn pawn in raidPawns)
            {
                if (tempDic.ContainsKey(pawn.kindDef))
                {
                    tempDic[pawn.kindDef]++;
                }
                else
                {
                    tempDic[pawn.kindDef] = 1;
                }
            }
            totalPawns = tempDic.Sum(k => k.Value);

            string kindLabel = "VESWW.EnemiesC".Translate(totalPawns) + "\n";
            kindListLines++;
            foreach (var pair in tempDic)
            {
                kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                kindListLines++;
            }
            kindList = kindLabel.TrimEndNewlines();

            ApplyModifiers();
        }
    }
}