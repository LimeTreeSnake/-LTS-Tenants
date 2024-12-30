using RimWorld;
using Verse;

namespace Tenants.Defs
{
	[DefOf]
	public static class LetterDefs
	{
		public static LetterDef LTS_TenancyJoinLetter;
		
		static LetterDefs() {
			DefOfHelper.EnsureInitializedInCtor(typeof(LetterDefOf));
		}
	}
}