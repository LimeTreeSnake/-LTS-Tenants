using Verse;
using RimWorld;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming
namespace Tenants.Defs {
    [DefOf]
    public static  class ThingDefOf {
        public static ThingDef LTS_NoticeBoard;
        public static ThingDef LTS_CourierCoat;
        public static ThingDef LTS_CourierHat;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}
