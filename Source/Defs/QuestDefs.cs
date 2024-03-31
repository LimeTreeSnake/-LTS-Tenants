using RimWorld;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace Tenants.Defs {
    [DefOf]
    public static class QuestDefOf {
        public static QuestScriptDef LTS_Tenancy;
        static QuestDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuestDefOf));
        }
    }
}
