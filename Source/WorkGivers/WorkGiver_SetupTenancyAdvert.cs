using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Tenants.Things;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.WorkGivers {
    public class WorkGiver_SetupTenancyAdvert : WorkGiver_Scanner {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
            Danger maxDanger = pawn.NormalMaxDanger();
            List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int j = 0; j < bList.Count; j++) {
                if (bList[j] is NoticeBoard building_NoticeBoard && building_NoticeBoard.isActive && !building_NoticeBoard.noticeUp && pawn.CanReach(building_NoticeBoard, PathEndMode.InteractionCell, maxDanger) && !building_NoticeBoard.IsBurning()) {
                    yield return bList[j];
                }
            }
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false) {
            if (pawn.GetLord() != null) {
                return true;
            }
            return base.ShouldSkip(pawn, forced);
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
            if (!t.Spawned) {
                return false;
            }
            if (t is NoticeBoard) {
                return CanAddNotice(pawn, t);
            }
            return true;
        }
        public static bool CanAddNotice(Pawn pawn, Thing t) {
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null)) {
                return false;
            }
            if (t.Faction != pawn.Faction) {
                return false;
            }
            NoticeBoard building_NoticeBoard = t as NoticeBoard;
            if (building_NoticeBoard.noticeUp || !building_NoticeBoard.isActive) {
                return false;
            }
            ThingDefCountClass thingDefCountClass = new ThingDefCountClass(ThingDefOf.Silver, Settings.Settings.NoticeCourierCost);
            if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(thingDefCountClass, pawn)) {
                return false;
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
            Job job = JobMaker.MakeJob(Defs.JobDefOf.LTS_AddNotice, t);
            bool needMore = true;
            job.playerForced = true;
            job.count = Settings.Settings.NoticeCourierCost;
            job.GetTargetQueue(TargetIndex.B);
            int counter = 0;
            Thing temp = null;
            while (needMore) {
                bool validator(Thing pay) {
                    if (!pay.Spawned) {
                        return false;
                    }
                    if (pay.IsForbidden(pawn)) {
                        return false;
                    }
                    if (!pawn.CanReserve(pay)) {
                        return false;
                    }
                    if (pawn.HasReserved(pay)) {
                        return false;
                    }
                    return true;
                }
                Thing thing = GenClosest.ClosestThing_Global_Reachable(temp == null ? pawn.Position : temp.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Silver), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, validator);
                if (thing != null) {
                    pawn.Reserve(thing, job);
                    job.targetQueueB.Add(thing);
                    counter += thing.stackCount;
                    temp = thing;
                    if (counter >= job.count) {
                        needMore = false;
                    }
                }
            }
            return job;
        }
    }
}
