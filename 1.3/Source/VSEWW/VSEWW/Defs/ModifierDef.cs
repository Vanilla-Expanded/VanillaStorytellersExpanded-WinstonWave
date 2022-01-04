using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

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
		}

        public void DrawCard(Rect rect)
		{
			Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width);
			GUI.DrawTexture(iconRect.ContractedBy(10), ModifierIcon);

			Widgets.DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, $"{label}:\n{description}");
		}
	}
}
