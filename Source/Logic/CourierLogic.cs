using Verse;
using RimWorld;
using Tenants.Components;
using System.Collections.Generic;
using System;
using Verse.AI.Group;
using System.Linq;

namespace Tenants.Logic
{
	public static class CourierLogic
	{
		public static bool CourierEvent(IncidentParms parms, Components.Tenants_MapComponent component)
		{
			try
			{
				var map = (Map)parms.target;
				RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 loc, map, CellFinder.EdgeRoadChance_Neutral);
				Pawn courier = component.GetCourier();
				if (courier == null)
				{
					return true;
				}

				if (!Settings.Settings.KillPenalty)
				{
					component.CourierKills = 0;
				}

				if (component.CourierKills > 0)
				{
					Find.LetterStack.ReceiveLetter(Language.Translate.CourierDenied,
						Language.Translate.CourierDeniedMessage(courier), LetterDefOf.NegativeEvent);

					Log.Message(component.CourierKills.ToString());
					component.CourierKills--;
				}
				else
				{
					foreach (Need t in courier.needs.AllNeeds)
					{
						t.CurLevelPercentage = 0.8f;
					}

					GenSpawn.Spawn(courier, loc, map);
					courier.relations.everSeenByPlayer = true;
					Find.LetterStack.ReceiveLetter(Language.Translate.CourierArrival,
						Language.Translate.CourierArrivalMessage(courier), LetterDefOf.PositiveEvent, courier);

					LordMaker.MakeNewLord(courier.Faction, new Lords.Courier_LordJob(), map, new List<Pawn>
					{
						courier
					});
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Message("Error at CourierEvent method: " + ex.Message);
				return false;
			}
		}
	}
}