using RimWorld;
using RimWorld.QuestGen;
using System;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyContract : QuestNode {
	    // ReSharper disable MemberCanBePrivate.Global
	    // ReSharper disable InconsistentNaming
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
		// ReSharper restore MemberCanBePrivate.Global
		// ReSharper restore InconsistentNaming

		protected override void RunInt() {
            try {
				Slate slate = QuestGen.slate;
				if (tenant.GetValue(slate) == null || map.GetValue(slate) == null) {
					return;
				}
				map.TryGetValue(slate, out Map mapStuff);
				tenant.TryGetValue(slate, out Pawn tenantTemp);
				Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
				Models.Contract contracts = Logic.TenancyLogic.GenerateBasicTenancyContract(tenantTemp);
				if (contract.GetValue(slate) == null)
				{
					return;
				}

				QuestGen.slate.Set(contract.GetValue(slate), contracts);
				QuestGen.slate.Set("days", contracts.LengthDays);
				QuestGen.slate.Set("rent", contracts._rent);
				QuestGen.slate.Set("rentSum", contracts._rent * contracts.LengthDays);
				QuestGen.slate.Set("ticks", contracts._length);
				QuestGen.slate.Set("startdate", contracts._startdate);
				QuestGen.slate.Set("endDate", contracts._endDate);
				QuestGen.slate.Set("roomRequired", contracts._singleRoomRequirement);
            } catch (Exception ex) {
				Log.Message("Error at QuestNode_Tenancy RunInt: " + ex.Message);
			}
		}

        protected override bool TestRunInt(Slate slate) {
            return slate.Exists("map");
        }
    }
}
