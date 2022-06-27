using RimWorld;
using RimWorld.QuestGen;
using System;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyRoom : QuestNode {
		[NoTranslate]
		public SlateRef<Map> map;
		public Models.Contract contract;
		protected override void RunInt() {

		}

        protected override bool TestRunInt(Slate slate) {
            return slate.Exists("map", false);
        }
    }
}
