using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;
using System.Text;

namespace Tenants.Lords {
    public class Courier_LordJob : LordJob {
        public override StateGraph CreateGraph() {
            StateGraph StateGraph = new StateGraph();
            LordToil toilTravel = new LordToil_Travel(Map.GetComponent<Components.Tenants_MapComponent>().NoticeBoard.InteractionCell) {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilTravel);
            LordToil toilDeliver = new CourierDeliver_LordToil();
            StateGraph.AddToil(toilDeliver);

            LordToil toilLeave = new LordToil_ExitMap() {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilLeave);
            Transition transitionWait = new Transition(toilTravel, toilDeliver);
            transitionWait.AddTrigger(new Trigger_Memo("TravelArrived"));
            StateGraph.AddTransition(transitionWait);
            Transition transitionLeave = new Transition(toilDeliver, toilLeave);
            transitionLeave.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
            StateGraph.AddTransition(transitionLeave);
            return StateGraph;
        }
    }

    public class CourierDeliver_LordToil : LordToil {
        public override bool ForceHighStoryDanger => false;
        public override bool AllowSelfTend => true;
        public override void UpdateAllDuties() {
            for (int i = 0; i < lord.ownedPawns.Count; i++) {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait);
            }
            Map.GetComponent<Components.Tenants_MapComponent>().EmptyBoard(lord.ownedPawns[0]);
        }
    }
}
