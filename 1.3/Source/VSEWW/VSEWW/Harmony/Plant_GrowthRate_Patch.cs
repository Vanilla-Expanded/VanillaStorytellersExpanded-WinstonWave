using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("GrowthRate", MethodType.Getter)]
    public class Plant_GrowthRate_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
                __result *= 3;
        }
    }
}
