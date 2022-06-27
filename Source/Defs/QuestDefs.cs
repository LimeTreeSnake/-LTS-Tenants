using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class QuestDefOf {
        public static QuestScriptDef Tenancy;
        static QuestDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuestDefOf));
        }
    }
}
