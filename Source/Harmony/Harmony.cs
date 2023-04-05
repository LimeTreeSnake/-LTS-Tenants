using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Components;
// ReSharper disable InconsistentNaming

namespace Tenants.Harmony
{
	[StaticConstructorOnStartup]
	public class Harmony
	{
		static Harmony()
		{
			var harmony = new HarmonyLib.Harmony("limetreesnake.tenants");
			harmony.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "get_IdleColonists"), null,
				new HarmonyMethod(typeof(Harmony).GetMethod("IdleColonists_PostFix")));

			harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill"), null,
				new HarmonyMethod(typeof(Harmony).GetMethod("Kill_PostFix")));
		}

		public static void IdleColonists_PostFix(ref List<Pawn> __result)
		{
			if (__result.Count <= 0)
			{
				return;
			}

			Tenants_MapComponent comp = __result[0].Map.GetComponent<Tenants_MapComponent>();
			if (comp == null)
			{
				return;
			}

			IEnumerable<Pawn> list = __result.Where(x => !comp.IsTenant(x)).ToList();
			__result = list.ToList();
		}

		public static void Kill_PostFix(ref Pawn __instance)
		{
			if (__instance?.Map == null || !__instance.Map.IsPlayerHome || !__instance.Spawned)
			{
				return;
			}

			Tenants_MapComponent comp = __instance.Map.GetComponent<Tenants_MapComponent>();
			if (comp != null && comp.IsCourier(__instance))
			{
				comp.CourierKills++;
			}
		}
	}
}