using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Components;
using Tenants.Language;
using UnityEngine;
using FactionDefOf = Tenants.Defs.FactionDefOf;

// ReSharper disable InconsistentNaming

namespace Tenants.Harmony
{
	[StaticConstructorOnStartup]
	public class Harmony
	{
		static Harmony()
		{
			var harmony = new HarmonyLib.Harmony("limetreesnake.tenants");

			harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill"),
				new HarmonyMethod(typeof(Harmony).GetMethod("Kill_PreFix")));
			
			harmony.Patch(AccessTools.Method(typeof(SignalManager), "SendSignal"),
				new HarmonyMethod(typeof(Harmony).GetMethod("SendSignal_PreFix")));
		}

		public static void SendSignal_PreFix(Signal signal)
		{
			if(Settings.Settings.DebugLog )
			{
				Log.Message(signal.ToString());
			}
		}
			
		public static void Kill_PreFix(Pawn __instance)
		{
			if (__instance?.Map == null )
			{
				return;
			}
			if (!__instance.Map.IsPlayerHome)
			{
				return;
			}
			if (!__instance.Spawned)
			{
				return;
			}
			if (__instance.Faction == null)
			{
				return;
			}

			TenantsMapComponent comp = __instance.Map.GetComponent<TenantsMapComponent>();

			if (comp == null)
			{
				return;
			}

			if (comp.IsCourier(__instance))
			{
				comp.CourierKills++;
				Messages.Message(Translate.LoneCourierDied(__instance), MessageTypeDefOf.NegativeEvent);					
				if (Settings.Settings.DebugLog)
				{
					Log.Message("KillPenalty Courier: " + comp.CourierKills);
				}
			}

			if (__instance.Faction.def != FactionDefOf.LTS_Tenant)
			{
				return;
			}
			
			comp.TenantKills++;
			Messages.Message(Translate.LoneTenantDied(__instance), MessageTypeDefOf.NegativeEvent);
			if (Settings.Settings.DebugLog)
			{
				Log.Message("KillPenalty Lone Tenants: " + comp.TenantKills);
			}
		}
	}
}