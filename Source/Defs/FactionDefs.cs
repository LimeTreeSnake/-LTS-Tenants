using RimWorld;

// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace Tenants.Defs {
    [DefOf]
    public static class FactionDefOf {
        public static FactionDef LTS_Courier;
        public static FactionDef LTS_Tenant;
        static FactionDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
        }
    }
}
