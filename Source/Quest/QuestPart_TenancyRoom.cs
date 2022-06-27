using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Tenants.QuestNodes {
    public class QuestPart_TenancyRoom : QuestPart_RequirementsToAccept {
        public Map map;
        public Models.Contract contract;
        //private List<Thing> tmpOccupiedBeds = new List<Thing>();

        //private List<Pawn> culpritsResult = new List<Pawn>();
        public override void Notify_QuestSignalReceived(Signal signal) {
        }
        public override void ExposeData() {
            base.ExposeData();
        }

        public override AcceptanceReport CanAccept() {
            throw new System.NotImplementedException();
        }
    }
}
