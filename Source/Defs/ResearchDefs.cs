using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class ResearchDefOf {
        public static ResearchProjectDef LTS_CourierTech;
        static ResearchDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchDefOf));
        }
    }
}
