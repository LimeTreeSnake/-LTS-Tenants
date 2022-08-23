using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Diagnostics.Contracts;

namespace Tenants.Alerts {
    public class Alert_TenancyBedroom : Alert {
        private readonly List<Pawn> culpritsResult = new List<Pawn>();
        public override AlertReport GetReport() {
            return AlertReport.CulpritsAre(Culprits);
        }
        public Alert_TenancyBedroom() {
            defaultLabel = Language.Translate.TenancyRoomRequired.Translate();
            defaultExplanation = Language.Translate.TenancyRoomRequiredDesc.Translate();
        }
        private List<Pawn> Culprits {
            get {
                culpritsResult.Clear();
                List<Map> maps = Find.Maps;
                if (maps.Any()) {
                    for (int i = 0; i < maps.Count; i++) {
                        Components.Tenants_MapComponent comp = maps[i].GetComponent<Components.Tenants_MapComponent>();
                        if (comp.ActiveContracts.Any()) {
                            for (int y = 0; y < comp.ActiveContracts.Count; y++) {
                                Models.Contract contract = comp.ActiveContracts[i];
                                if (contract.tenant != null && contract.tenant.HasExtraHomeFaction()) {
                                    if (contract.singleRoomRequirement && !contract.tenant.royalty.HasPersonalBedroom()) {
                                        culpritsResult.Add(contract.tenant);
                                    }
                                }
                                else {
                                    comp.ActiveContracts.Remove(contract);
                                }
                            }
                        }
                    }
                }
                return culpritsResult;
            }
        }

    }
}
