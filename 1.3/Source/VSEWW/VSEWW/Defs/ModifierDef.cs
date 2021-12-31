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

		public virtual void DrawCard(Rect rect, Window window, Map map)
		{
			Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width);
			GUI.DrawTexture(iconRect.ContractedBy(10), ModifierIcon);

			Widgets.DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, (TipSignal)description);
		}
	}
}
