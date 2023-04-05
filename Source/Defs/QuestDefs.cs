using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace Tenants.Defs {
    [DefOf]
    public static class QuestDefOf {
        public static QuestScriptDef Tenancy;
        static QuestDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuestDefOf));
        }
    }
}
