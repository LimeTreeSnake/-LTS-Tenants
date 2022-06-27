using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Components;

namespace Tenants.Harmony {
    [StaticConstructorOnStartup]
    public class Harmony {
        static Harmony() {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("limetreesnake.tenants");
            harmony.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "get_IdleColonists"), null, new HarmonyMethod(typeof(Harmony).GetMethod("IdleColonists_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill"), null, new HarmonyMethod(typeof(Harmony).GetMethod("Kill_PostFix")));
        }
        public static void IdleColonists_PostFix(ref List<Pawn> __result) {
            if (__result.Count > 0) {
                Tenants_MapComponent comp = Tenants_MapComponent.GetComponent(__result[0].Map);
                IEnumerable<Pawn> list = __result.Where(x => !comp.IsTenant(x)).ToList();
                __result = list != null ? list.ToList() : __result;
            }
        }
        public static void Kill_PostFix(ref Pawn __instance) {
            if (__instance != null && __instance.Map != null && __instance.Map.IsPlayerHome && __instance.Spawned) {
                Tenants_MapComponent comp = Tenants_MapComponent.GetComponent(__instance.Map);
                if (comp.IsCourier(__instance)) {
                    comp.CourierKills++;
                }
            }
        }
    }
}