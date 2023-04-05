using RimWorld;
using Verse;

namespace Tenants.IncidentWorkers
{
	public class CourierArrival_IncidentWorker : IncidentWorker
	{

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}

			var map = (Map)parms.target;
			if (map == null)
			{
				return false;
			}

			Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
			if (comp == null)
			{
				map.components.Add(new Components.Tenants_MapComponent(map));
			}

			return comp != null &&
			       comp.FindNoticeBoardInMap() &&
			       RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map,
				       CellFinder.EdgeRoadChance_Neutral);

		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{

			var map = (Map)parms.target;
			if (map == null)
			{
				return false;
			}

			Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
			if (comp == null)
			{
				map.components.Add(new Components.Tenants_MapComponent(map));
			}

			if (comp?.NoticeBoard != null && comp.NoticeBoard.Spawned)
			{
				return Logic.CourierLogic.CourierEvent(parms, comp);
			}

			return false;
		}
	}
}