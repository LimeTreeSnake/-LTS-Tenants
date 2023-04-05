using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace Tenants.Defs {
    [DefOf]
    public static class HistoryEventDefOf {
        public static HistoryEventDef TenancyLeave;
        public static HistoryEventDef TenancyDeath;
        public static HistoryEventDef TenancyJoin;
        static HistoryEventDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(HistoryEventDefOf));
        }
    }
}
