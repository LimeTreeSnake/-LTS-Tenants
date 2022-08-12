using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Tenants.QuestNodes {
    public class QuestPart_TenancyRoom : QuestPart_RequirementsToAccept {
        public Models.Contract contract;
        private readonly List<Pawn> culpritsResult = new List<Pawn>();
        public override void Notify_QuestSignalReceived(Signal signal) {
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref contract, "Contract");
        }
        private List<Pawn> CulpritsAre() {
            culpritsResult.Clear();
            if (contract.singleRoomRequirement && contract.tenant != null) {
                if (contract.tenant.Spawned && !contract.tenant.royalty.HasPersonalBedroom()) {
                    culpritsResult.Add(contract.tenant);
                }
            } 
            return culpritsResult;
        }

        public override AcceptanceReport CanAccept() {
            int num = CulpritsAre().Count();
            if (num > 0) {
                return ("QuestBedroomRequirementsUnsatisfiedSingle").Translate();
            }
            return true;
        }
    }
}
