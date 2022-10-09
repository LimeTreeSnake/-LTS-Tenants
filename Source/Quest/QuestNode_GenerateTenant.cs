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
        public SlateRef<string> race;
        [NoTranslate]
        public SlateRef<string> gender;
        [NoTranslate]
        public SlateRef<string> genes;
        [NoTranslate]
        public SlateRef<string> addToList;
        [NoTranslate]
        public SlateRef<string> tenantFaction;
        [NoTranslate]
        public SlateRef<bool> rejected;
        public SlateRef<Map> map;
        protected override void RunInt() {
            try {
                Slate slate = QuestGen.slate;
                if (this.map.GetValue(slate) == null) {
                    Log.Error("No map found for the quest slate");
                    return;
                }
                map.TryGetValue(slate, out Map mapStuff);
                Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
                if (comp == null) {
                    mapStuff.components.Add(new Components.Tenants_MapComponent(mapStuff));
                }
                Pawn newTenant = comp.GetTenant();
                if (newTenant != null) {
                    if (this.tenant.GetValue(slate) != null) {
                        QuestGen.slate.Set(tenant.GetValue(slate), newTenant, false);
                    }
                    if (this.race.GetValue(slate) != null) {
                        QuestGen.slate.Set(race.GetValue(slate), newTenant.def.label, false);
                    }
                    if (this.gender.GetValue(slate) != null) {
                        QuestGen.slate.Set(gender.GetValue(slate), newTenant.gender.GetLabel(), false);
                    }
                    if (this.genes.GetValue(slate) != null) {
                        if (ModLister.BiotechInstalled) {
                            QuestGen.slate.Set(genes.GetValue(slate), newTenant.genes.XenotypeLabel, false);
                        }
                        else {
                            QuestGen.slate.Set(genes.GetValue(slate), "", false);
                        }
                    }
                    if (this.addToList.GetValue(slate) != null) {
                        QuestGenUtility.AddToOrMakeList(QuestGen.slate, addToList.GetValue(slate), newTenant);
                    }
                    if (this.tenantFaction.GetValue(slate) != null) {
                        QuestGen.slate.Set(tenantFaction.GetValue(slate), newTenant.Faction, false);
                    }
                    if (!Settings.Settings.KillPenalty)
                        comp.TenantKills = 0;
                    QuestGen.slate.Set("rejected", comp.TenantKills > 0, false);
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
