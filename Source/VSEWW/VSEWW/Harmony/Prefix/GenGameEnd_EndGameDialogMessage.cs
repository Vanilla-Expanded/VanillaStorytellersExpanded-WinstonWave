using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VSEWW
{
    [HarmonyPatch(typeof(GenGameEnd))]
    [HarmonyPatch("EndGameDialogMessage")]
    [HarmonyPatch(MethodType.Normal, new Type[] { typeof(string), typeof(bool) })]
    public class GenGameEnd_EndGameDialogMessage
    {
        [HarmonyPrefix]
        public static bool Prefix(string msg, bool allowKeepPlaying)
        {
            if (Find.Storyteller.def.defName == "VSE_WinstonWave")
            {
                Map map = Find.CurrentMap;
                if (map.GetComponent<MapComponent_Winston>() is MapComponent_Winston mapComp && mapComp != null)
                {
                    Find.WindowStack.Add(new Window_GameOver(msg, allowKeepPlaying));
                    return false;
                }
                return true;
            }
            return true;
        }
    }
}