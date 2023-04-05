using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming
namespace Tenants.Defs {
    [DefOf]
    public static class JobDefOf {

        public static JobDef LTS_AddNotice;

        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}
