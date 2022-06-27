using RimWorld;
using RimWorld.QuestGen;
using System;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyFailure : QuestNode {
		[NoTranslate]
		public SlateRef<Map> map;

		protected override void RunInt() {
            try {
				Slate slate = QuestGen.slate;
				if (this.map.GetValue(slate) == null) {
					return;
				}
				map.TryGetValue(slate, out Map mapStuff);			
				Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
				comp.TenantKills++;
			} catch (Exception ex) {
				Log.Message("Error at QuestNode_TenancyFailure RunInt: " + ex.Message);
			}
		}

        protected override bool TestRunInt(Slate slate) {
            return slate.Exists("map", false);
        }
    }
}
