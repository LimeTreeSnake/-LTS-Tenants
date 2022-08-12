using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Tenants.IncidentWorkers {
    public class CourierArrival_IncidentWorker : IncidentWorker {

        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if(map != null) {
                    Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                    if (comp == null) {
                        map.components.Add(new Components.Tenants_MapComponent(map));
                    }
                    if (comp.FindNoticeBoardInMap() && RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, false, null)) {
                        return true;
                    }
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms) {
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                    if (comp == null) {
                        map.components.Add(new Components.Tenants_MapComponent(map));
                    }
                    if (comp.NoticeBoard != null && comp.NoticeBoard.Spawned) {
                        return Logic.CourierLogic.CourierEvent(parms, comp);
                    }
                }
            }
            return false;
        }
    }
}
