using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VSEWW
{
    public class IncidentWorker_MakeGameConditionNoLetter : IncidentWorker_MakeGameCondition
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            GameConditionManager conditionManager = parms.target.GameConditionManager;
            GameCondition gameCondition = GameConditionMaker.MakeCondition(this.def.gameCondition, Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f));

            conditionManager.RegisterCondition(gameCondition);
            return true;
        }
    }
}
