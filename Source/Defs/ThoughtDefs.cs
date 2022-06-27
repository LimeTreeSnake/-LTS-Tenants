using Verse;
using RimWorld;
namespace Tenants.Defs {
    [DefOf]
    public static class ThoughtDefOf {
        public static ThoughtDef LTS_JoinRejection;
        public static ThoughtDef LTS_JoinAccept;
        static ThoughtDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
        }
    }
}
