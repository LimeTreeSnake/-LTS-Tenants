using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;
using System.Text;

namespace Tenants.Lords
{
	public class Courier_LordJob : LordJob
	{
		public override StateGraph CreateGraph()
		{
			var stateGraph = new StateGraph();
			LordToil toilTravel =
				new LordToil_Travel(this.Map.GetComponent<Components.Tenants_MapComponent>().NoticeBoard.InteractionCell)
				{
					useAvoidGrid = true
				};

			stateGraph.AddToil(toilTravel);
			LordToil toilDeliver = new CourierDeliver_LordToil();
			stateGraph.AddToil(toilDeliver);

			LordToil toilLeave = new LordToil_ExitMap()
			{
				useAvoidGrid = true
			};

			stateGraph.AddToil(toilLeave);
			var transitionWait = new Transition(toilTravel, toilDeliver);
			transitionWait.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transitionWait);
			var transitionLeave = new Transition(toilDeliver, toilLeave);
			transitionLeave.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
			stateGraph.AddTransition(transitionLeave);
			return stateGraph;
		}
	}

	public class CourierDeliver_LordToil : LordToil
	{
		public override bool ForceHighStoryDanger => false;
		public override bool AllowSelfTend => true;

		public override void UpdateAllDuties()
		{
			foreach (Pawn t in this.lord.ownedPawns)
			{
				t.mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait);
			}

			this.Map.GetComponent<Components.Tenants_MapComponent>().EmptyBoard(this.lord.ownedPawns[0]);
		}
	}
}