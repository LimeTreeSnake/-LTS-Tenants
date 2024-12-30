using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using Verse.AI.Group;

namespace Tenants.Logic
{
	public static class CourierLogic
	{
		private static List<PawnKindDef> _availablePawnKinds;

		private static void Initiate()
		{
			_availablePawnKinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x =>
					x.HasModExtension<DefModExtensions.CourierExtension>())
				.ToList();
		}

		public static PawnKindDef GetRandomPawnKindDef()
		{
			if (_availablePawnKinds.NullOrEmpty())
			{
				Initiate();
			}

			return _availablePawnKinds.RandomElementByWeight(x =>
				x.GetModExtension<DefModExtensions.CourierExtension>().choiceWeight);
		}

		public static bool CourierEvent(IncidentParms parms, Components.TenantsMapComponent component)
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
				Log.Error($"LTS_Tenants Error - CourierEvent: {ex.Message}\n{ex.StackTrace}");
				return false;
			}
		}
	}
}