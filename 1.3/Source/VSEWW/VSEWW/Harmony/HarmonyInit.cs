using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace VSEWW
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.VESWinstonWave");
            harmonyInstance.PatchAll();
        }
    }
}
