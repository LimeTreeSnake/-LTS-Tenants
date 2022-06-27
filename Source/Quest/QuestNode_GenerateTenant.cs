using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Linq;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_GenerateTenant : QuestNode {
        [NoTranslate]
        public SlateRef<string> tenant;
        [NoTranslate]
        public SlateRef<string> addToList;
        [NoTranslate]
        public SlateRef<string> tenantFaction;
        public SlateRef<Map> map;
        [NoTranslate]
        public SlateRef<string> rejected;
        protected override void RunInt() {
            try {
               Slate slate = QuestGen.slate;
                if (this.map.GetValue(slate) == null) {
                    Log.Error("No map found for the quest slate");
                    return;
                }
                map.TryGetValue(slate, out Map mapStuff);
                Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
                Pawn newTenant = comp.GetTenant();
                //if (!Settings.Settings.KillPenalty)
                comp.TenantKills = 0;
                if (comp.TenantKills > 0) {
                    Find.LetterStack.ReceiveLetter(Language.Translate.CourierDenied, Language.Translate.TenancyDeniedMessage(newTenant), LetterDefOf.NegativeEvent);
                    comp.TenantKills--;
                    QuestGen.slate.Set<bool>(this.rejected.GetValue(slate), true, false);
                }
                else if (newTenant != null) {
                    if (this.tenant.GetValue(slate) != null) {
                        QuestGen.slate.Set<Pawn>(this.tenant.GetValue(slate), newTenant, false);
                    }
                    if (this.addToList.GetValue(slate) != null) {
                        QuestGenUtility.AddToOrMakeList(QuestGen.slate, this.addToList.GetValue(slate), newTenant);
                    }
                    if (this.tenantFaction.GetValue(slate) != null) {
                        QuestGen.slate.Set<Faction>(this.tenantFaction.GetValue(slate), newTenant.Faction, false);
                    }
                }
            }
            catch (Exception ex) {
                Log.Message("Error at QuestNode_GenerateTenant RunInt: " + ex.Message);
            }
        }

        protected override bool TestRunInt(Slate slate) {
            return slate.Exists("map", false);
        }
    }
}
