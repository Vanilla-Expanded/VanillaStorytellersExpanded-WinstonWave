using RimWorld;
using UnityEngine;

namespace VSEWW
{
    public class IncidentWorker_MakeGameConditionNoLetter : IncidentWorker_MakeGameCondition
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var gameCondition = GameConditionMaker.MakeCondition(def.gameCondition, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f));
            parms.target.GameConditionManager.RegisterCondition(gameCondition);
            return true;
        }
    }
}