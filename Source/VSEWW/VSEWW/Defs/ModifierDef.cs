using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    public class ModifierDef : Def
    {
        public string texPath;

        // Multiply points by
        public float pointMultiplier = 0f;

        // Hediff not applied to a part
        public List<HediffDef> globalHediffs;

        // Hediff applied to specific part
        public List<ThingDef> techHediffs;

        // Retreat ?
        public bool everRetreat = true;

        // Incidents to fire
        public List<IncidentDef> incidents;

        // Weapons choice
        public List<ThingCategoryDef> allowedWeaponCategory;

        public List<ThingDef> allowedWeaponDef;

        // Apparels choice
        public List<ThingDef> neededApparelDef;

        // Don't use with those other modifiers
        public List<ModifierDef> incompatibleWith = new List<ModifierDef>();

        // Use specific pawnkind
        public List<PawnKindDef> specificPawnKinds;

        // RANDOM
        public bool mystery = false;

        private Texture2D modifierIcon;

        public Texture2D ModifierIcon
        {
            get
            {
                if (modifierIcon is null)
                {
                    modifierIcon = texPath == null ? null : ContentFinder<Texture2D>.Get(texPath, false);
                    if (modifierIcon is null)
                    {
                        modifierIcon = BaseContent.BadTex;
                    }
                }
                return modifierIcon;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in base.ConfigErrors())
                yield return str;
            if (description == null)
                yield return $"ModifierDef {defName} has null description";
            if (label == null)
                yield return $"ModifierDef {defName} has null label";
            if (!allowedWeaponCategory.NullOrEmpty() && !allowedWeaponDef.NullOrEmpty())
                yield return $"{defName} can't have allowedWeaponCategory && allowedWeaponDef";
        }

        public void DrawCard(Rect rect)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width);
            GUI.DrawTexture(iconRect, Startup.ModifierBGTex);
            GUI.DrawTexture(iconRect.ContractedBy(10), ModifierIcon);
            TooltipHandler.TipRegion(rect, $"<b>{label}</b>\n{description}");
        }
    }
}