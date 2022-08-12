using RimWorld;
using RimWorld.QuestGen;
using System;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyContract : QuestNode {
		[NoTranslate]
		public SlateRef<string> contract;
		[NoTranslate]
		public SlateRef<int> ticks;
		[NoTranslate]
		public SlateRef<int> startdate;
		[NoTranslate]
		public SlateRef<int> endDate;
		[NoTranslate]
		public SlateRef<int> days;
		[NoTranslate]
		public SlateRef<int> rent;
		[NoTranslate]
		public SlateRef<int> rentSum;
		[NoTranslate]
		public SlateRef<bool> roomRequired;
		public SlateRef<Pawn> tenant;
		public SlateRef<Map> map;

		protected override void RunInt() {
            try {
				Slate slate = QuestGen.slate;
				if (this.tenant.GetValue(slate) == null || this.map.GetValue(slate) == null) {
					return;
				}
				map.TryGetValue(slate, out Map mapStuff);
				tenant.TryGetValue(slate, out Pawn tenantTemp);
				Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
				Models.Contract contract = Logic.TenancyLogic.GenerateBasicTenancyContract(tenantTemp);
				if (this.contract.GetValue(slate) != null) {
					QuestGen.slate.Set(this.contract.GetValue(slate), contract, false);
					QuestGen.slate.Set("days", contract.LengthDays, false);
					QuestGen.slate.Set("rent", contract.rent, false);
					QuestGen.slate.Set("rentSum", contract.rent * contract.LengthDays, false);
					QuestGen.slate.Set("ticks", contract.length, false);
					QuestGen.slate.Set("startdate", contract.startdate, false);
					QuestGen.slate.Set("endDate", contract.endDate, false);
					QuestGen.slate.Set("roomRequired", contract.singleRoomRequirement, false);
				}				
			} catch (Exception ex) {
				Log.Message("Error at QuestNode_Tenancy RunInt: " + ex.Message);
			}
		}

        protected override bool TestRunInt(Slate slate) {
            return slate.Exists("map", false);
        }
    }
}
