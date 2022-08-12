using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Tenants.Alerts {
    public class Alert_TenancyBedroom : Alert {
        private readonly List<Pawn> culpritsResult = new List<Pawn>();
        public override AlertReport GetReport() {
            return AlertReport.CulpritsAre(Culprits);
        }
        public Alert_TenancyBedroom() {
            defaultLabel = Language.Translate.TenancyRoomRequired.Translate();
            defaultExplanation = Language.Translate.TenancyRoomRequiredDesc.Translate();
            ;
        }
        public List<Pawn> Culprits {
            get {
                culpritsResult.Clear();
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++) {
                    Components.Tenants_MapComponent comp = maps[i].GetComponent<Components.Tenants_MapComponent>();
                    for (int y = 0; y < comp.ActiveContracts.Count; y++) {
                        if (comp.ActiveContracts[i].singleRoomRequirement && !comp.ActiveContracts[i].tenant.royalty.HasPersonalBedroom()) {
                            culpritsResult.Add(comp.ActiveContracts[i].tenant);
                        }
                    }
                }
                return culpritsResult;
            }
        }

    }
}
