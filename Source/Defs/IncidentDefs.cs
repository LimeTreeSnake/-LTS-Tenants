using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef CourierArrival;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
}
