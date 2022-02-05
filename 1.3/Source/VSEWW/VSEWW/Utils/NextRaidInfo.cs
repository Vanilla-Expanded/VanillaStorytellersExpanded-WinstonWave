using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VSEWW
{
    internal class NextRaidInfo : IExposable
    {
        // Raid infos
        // - Is always false at start - Set to true when lord isn't null
        public bool sent = false;
        private List<Lord> lords;
        // - When
        public int atTick;
        public int generatedAt;
        public int sentAt = 0;
        // - All modifiers applied to the raid
        public List<ModifierDef> modifiers = new List<ModifierDef>();
        public int? modifierCount;
        public bool reinforcementSent = false;
        // - Raid parms
        public IncidentParms incidentParms;
        public int reinforcementSeed = -1;
        // - Raid pawns
        public List<Pawn> raidPawns = new List<Pawn>();
        // - Wave number
        public int waveNum;
        private int? waveType;

        // Utils for wave counter
        // - Wave prediction string && size
        public string kindList;
        public int kindListLines;
        // - Wave progress cached alive pawns
        private int cacheTick = 0;
        private List<Pawn> lordPawnsCache = new List<Pawn>();
        internal string cacheKindList;
        // - Number of pawns at the start
        public int totalPawn;
        // Mystery
        public List<ModifierDef> mysteryModifier;

        // CE Loaded?
        public bool? ceActive = null;

        public bool CEActive
        {
            get
            {
                if (ceActive.HasValue)
                    return ceActive.Value;

                ceActive = ModsConfig.IsActive("CETeam.CombatExtended");
                return ceActive.Value;
            }
        }

        public List<Lord> Lords
        {
            get
            {
                if (!lords.NullOrEmpty())
                    return lords;

                Map map = (Map)incidentParms.target;
                if (map.lordManager != null && !map.lordManager.lords.NullOrEmpty())
                {
                    lords = map.lordManager.lords.FindAll(l => l.faction != null && l.faction == incidentParms.faction && !l.ownedPawns.NullOrEmpty() && l.AnyActivePawn);
                }

                return lords ?? null;
            }
        }

        public int WaveType
        {
            get
            {
                if (waveType != null)
                    return waveType.Value;

                waveType = waveNum % 5 == 0 ? 1 : 0;

                return waveType.Value;
            }
        }

        public int ModifierCount
        {
            get
            {
                if (modifierCount != null)
                    return modifierCount.Value;

                modifierCount = modifiers.Count;

                return modifierCount.Value;
            }
        }

        public bool Reinforcements => !reinforcementSent ? modifiers.Any(m => m.defName == "VSEWW_Reinforcements") : false;

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

        public void ExposeData()
        {
            Scribe_Values.Look(ref sent, "sent");
            Scribe_Values.Look(ref atTick, "atTick");
            Scribe_Values.Look(ref sentAt, "sentAt");
            Scribe_Collections.Look(ref modifiers, "modifiers");
            Scribe_Values.Look(ref reinforcementSent, "reinforcementSent", false);
            Scribe_Deep.Look(ref incidentParms, "incidentParms");
            Scribe_Values.Look(ref waveNum, "waveNum");
            Scribe_Values.Look(ref kindList, "kindList");
            Scribe_Values.Look(ref kindListLines, "kindListLines");
            Scribe_Values.Look(ref cacheTick, "cacheTick");
            Scribe_Values.Look(ref cacheKindList, "cacheKindList");
            Scribe_Values.Look(ref totalPawn, "totalPawn");
            Scribe_Values.Look(ref totalPawn, "totalPawn");
            Scribe_Values.Look(ref reinforcementSeed, "reinforcementSeed", -1);
            Scribe_Collections.Look(ref lords, "lords", LookMode.Reference);
            Scribe_Collections.Look(ref mysteryModifier, "mysteryModifier", LookMode.Def);

            if (lords.NullOrEmpty()) // Pawns are not part of a lord yet, else we don't save them, they are saved in the lord(s)
                Scribe_Collections.Look(ref raidPawns, "raidPawns", LookMode.Deep); // Deep save them
        }

        /** Get IRL time before this raid **/
        public string TimeBeforeWave() => TimeSpanExtension.Verbose(TimeSpan.FromSeconds((atTick - Find.TickManager.TicksGame).TicksToSeconds()));

        /** Get all pawns part of the raid - with caching **/
        public List<Pawn> WavePawns()
        {
            if ((lordPawnsCache.NullOrEmpty() is bool wasEmpty && wasEmpty) || cacheTick % 600 == 0)
            {
                string kindLabel = "VESWW.EnemiesR".Translate() + "\n";
                lordPawnsCache = new List<Pawn>();
                Dictionary<PawnKindDef, int> toDefeat = new Dictionary<PawnKindDef, int>();
                lords.ForEach(l =>
                {
                    lordPawnsCache.AddRange(l.ownedPawns.FindAll(p => !p.Downed && !p.mindState.mentalStateHandler.InMentalState));
                });
                lordPawnsCache.ForEach(p =>
                {
                    if (toDefeat.ContainsKey(p.kindDef))
                        toDefeat[p.kindDef]++;
                    else
                        toDefeat.Add(p.kindDef, 1);
                });
                foreach (var pair in toDefeat)
                {
                    kindLabel += $"{pair.Value} {pair.Key.LabelCap}\n";
                }
                cacheKindList = kindLabel.TrimEndNewlines();

                if (wasEmpty)
                    totalPawn = lordPawnsCache.Count;

                if (Reinforcements && lordPawnsCache.Count <= (int)(totalPawn * 0.8f))
                {
                    reinforcementSent = true;
                    ++Find.StoryWatcher.statsRecord.numRaidsEnemy;
                    var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, incidentParms.target);
                    parms.faction = incidentParms.faction;
                    parms.points = Math.Max(100f, incidentParms.points * 0.5f);
                    parms.pawnGroupMakerSeed = new Random().Next(1, 10000);
                    parms.customLetterLabel = "VESWW.Reinforcement".Translate();
                    reinforcementSeed = parms.pawnGroupMakerSeed.Value;
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                }
            }

            cacheTick++;
            return lordPawnsCache;
        }

        /** Get pawns count left **/
        public int WavePawnsLeft() => WavePawns().Count;

        /** Get modifiers chance **/
        private int[] GetModifiersChance()
        {
            int modifierChance = 0;

            if (WaveType == 1) modifierChance += 10;

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

        /** Choose and add modifier(s) **/
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
                    modifiersPool.Remove(modi);
                    modifiersPool.RemoveAll(m => m.incompatibleWith.Contains(modi));
                }
                    

                if (modifiersChance[1] > 0 && modifiersChance[1] < rand.Next(0, 100) && modifiersPool.Count > 0)
                    modifiers.Add(modifiersPool.RandomElement());

                ApplyModifiers(true);
            }
        }

        public List<ModifierDef> GetModifiersPool()
        {
            var modifiersPool = DefDatabase<ModifierDef>.AllDefsListForReading.FindAll(m => !VESWWMod.settings.modifierDefs.Contains(m.defName));
            modifiersPool.RemoveAll(m => m.pointMultiplier > 0 && (m.pointMultiplier * incidentParms.points) > VESWWMod.settings.maxPoints);

            if (!incidentParms.faction.def.humanlikeFaction)
            {
                modifiersPool.RemoveAll(m => !m.allowedWeaponDef.NullOrEmpty() ||
                                             !m.allowedWeaponCategory.NullOrEmpty() ||
                                             !m.neededApparelDef.NullOrEmpty() ||
                                             !m.techHediffs.NullOrEmpty() ||
                                             !m.globalHediffs.NullOrEmpty() ||
                                             !m.everRetreat);
            }

            return modifiersPool;
        }

        /** Apply modifier(s) **/
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

        public void ApplyModifier(ModifierDef modifier, bool first = false)
        {
            if (first)
            {
                if (modifier.pointMultiplier > 0) // Can only be applied before raid is sent
                    incidentParms.points *= modifier.pointMultiplier;

                if (!modifier.everRetreat)
                {
                    incidentParms.canTimeoutOrFlee = false;
                }

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
                            if (CEActive)
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
                            if (CEActive)
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

                        if (!modifier.neededApparelDef.NullOrEmpty())
                        {
                            foreach (var apparelDef in modifier.neededApparelDef)
                            {
                                if (!pawn.apparel.WornApparel.Any(a => a.def == apparelDef))
                                {
                                    ThingStuffPair apparel = ThingStuffPair.AllWith(a => a.IsApparel && a == apparelDef).RandomElement();
                                    if (apparel != null)
                                        pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(apparel.thing, apparel.stuff));
                                }
                            }
                        }
                    }
                }
            }
        }

        /** Send non-pawn modifier **/
        public void SendAddditionalModifier()
        {
            foreach (var modifier in modifiers)
            {
                if (!modifier.incidents.NullOrEmpty())
                {
                    modifier.incidents.ForEach(i =>
                    {
                        Find.Storyteller.incidentQueue.Add(i, Find.TickManager.TicksGame, new IncidentParms()
                        {
                            target = incidentParms.target
                        });
                    });
                }
            }
        }

        /** Stop non-pawn modifier **/
        public void StopEvents()
        {
            foreach (var modifier in modifiers)
            {
                if (!modifier.incidents.NullOrEmpty())
                {
                    Map map = (Map)incidentParms.target;
                    map.GameConditionManager.ActiveConditions.FindAll(g => modifier.incidents.Any(i => i.gameCondition != null && i.gameCondition == g.def)).ForEach(c => c.End());
                }
            }
        }

        /** Install part on pawn - copy of vanilla private method **/
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

        /** Set pawns prediction string and count **/
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
            totalPawn = tempDic.Sum(k => k.Value);

            string kindLabel = "VESWW.EnemiesC".Translate(totalPawn) + "\n";
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
