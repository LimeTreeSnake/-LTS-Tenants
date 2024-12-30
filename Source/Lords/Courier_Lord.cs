using System;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Lords
{
	public class Courier_LordJob : LordJob
	{
		public override StateGraph CreateGraph()
		{
			var stateGraph = new StateGraph();
			try
			{
				LordToil toilTravel =
					new LordToil_Travel(this.Map.GetComponent<Components.TenantsMapComponent>()
						.NoticeBoard()
						.InteractionCell)
					{
						useAvoidGrid = true
					};
				stateGraph.AddToil(toilTravel);
				
				LordToil toilDeliver = new CourierDeliver_LordToil();
				stateGraph.AddToil(toilDeliver);

				LordToil toilLeave = new LordToil_ExitMap(LocomotionUrgency.Jog)
				{
					useAvoidGrid = true
				};
				stateGraph.AddToil(toilLeave);

				LordToil toilDefendSelfPre = new LordToil_DefendSelf();
				
				LordToil toilDefendSelfPost = new LordToil_DefendSelf();
				
				var transitionWait = new Transition(toilTravel, toilDeliver);
				transitionWait.AddTrigger(new Trigger_Memo("TravelArrived"));
				stateGraph.AddTransition(transitionWait);
				
				var transitionLeave = new Transition(toilDeliver, toilLeave);
				transitionLeave.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
				stateGraph.AddTransition(transitionLeave);
				
				var transitionDefendGo = new Transition(toilTravel, toilDefendSelfPre);
				transitionDefendGo.AddTrigger(new Trigger_PawnHarmed());
				stateGraph.AddTransition(transitionDefendGo);
				
				var transitionDefendLeave = new Transition(toilDeliver, toilDefendSelfPost);
				transitionDefendLeave.AddTrigger(new Trigger_PawnHarmed());
				stateGraph.AddTransition(transitionDefendLeave);
				
				var transitionDefendHold = new Transition(toilLeave, toilDefendSelfPost);
				transitionDefendHold.AddTrigger(new Trigger_PawnHarmed());
				stateGraph.AddTransition(transitionDefendHold);
				
				var transitionTravel = new Transition(toilDefendSelfPre, toilTravel);
				transitionTravel.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
				stateGraph.AddTransition(transitionTravel);
				
				var transitionLeaveFinished = new Transition(toilDefendSelfPost, toilLeave);
				transitionLeaveFinished.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
				stateGraph.AddTransition(transitionLeaveFinished);
				
				
			}
			catch (Exception ex)
			{
				Log.Error($"LTS_Tenants Error - Courier_LordJob Failed to create graph: {ex.Message}\n{ex.StackTrace}");
			}

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

			this.Map.GetComponent<Components.TenantsMapComponent>().EmptyBoard(this.lord.ownedPawns[0]);
		}
	}
}