using Verse;
using RimWorld;
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
