using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace Tenants.Defs {
    [DefOf]
    public static class ResearchDefOf {
        public static ResearchProjectDef LTS_CourierTech;
        static ResearchDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchDefOf));
        }
    }
}
