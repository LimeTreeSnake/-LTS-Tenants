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

			Components.TenantsMapComponent comp = map.GetComponent<Components.TenantsMapComponent>();
			if (comp == null)
			{
				map.components.Add(new Components.TenantsMapComponent(map));
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

			Components.TenantsMapComponent comp = map.GetComponent<Components.TenantsMapComponent>();
			if (comp == null)
			{
				map.components.Add(new Components.TenantsMapComponent(map));
			}

			return comp?.NoticeBoard()?.Spawned == true && Logic.CourierLogic.CourierEvent(parms, comp);

		}
	}
}