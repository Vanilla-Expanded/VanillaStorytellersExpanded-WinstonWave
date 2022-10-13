using RimWorld;
using UnityEngine;

namespace VSEWW
{
    public class IncidentWorker_MakeGameConditionNoLetter : IncidentWorker_MakeGameCondition
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            GameConditionManager conditionManager = parms.target.GameConditionManager;
            GameCondition gameCondition = GameConditionMaker.MakeCondition(def.gameCondition, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f));

            conditionManager.RegisterCondition(gameCondition);
            return true;
        }
    }
}