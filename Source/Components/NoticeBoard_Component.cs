    using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using System;
using System.Text;
using LTS_Systems.Language;

namespace Tenants.Components {
    public class NoticeBoard_Component : ThingComp {
        public bool noticeForTenancy;

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref noticeForTenancy, "NoticeForTenancy");
        }
        public override string CompInspectStringExtra() {
            StringBuilder builder = new StringBuilder();
            if (noticeForTenancy)
                return Language.Translate.AdvertisementPlaced;
            else
                return string.Empty;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            if (pawn.Faction.IsPlayer && pawn.RaceProps.intelligence > Intelligence.Animal) {
                if (!noticeForTenancy) {
                    ThingDefCountClass thingDefCountClass = new ThingDefCountClass(ThingDefOf.Silver, Settings.Settings.NoticeCourierCost);
                    if (pawn.Map.itemAvailability.ThingsAvailableAnywhere(thingDefCountClass, pawn)) {
                        yield return new FloatMenuOption(Language.Translate.AddNoticeForTenancy(Settings.Settings.NoticeCourierCost), delegate {
                            Job job = JobMaker.MakeJob(Defs.JobDefOf.LTS_AddNotice, this.parent);
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
                                if (thing == null) {
                                    Messages.Message(Language.Translate.AdvertisementFailed(pawn), MessageTypeDefOf.NegativeEvent);
                                    return;
                                }
                                else {
                                    pawn.Reserve(thing, job);
                                    job.targetQueueB.Add(thing);
                                    counter += thing.stackCount;
                                    temp = thing;
                                    if (counter >= job.count) {
                                        needMore = false;
                                    }
                                }
                            }
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
                        });
                    }
                }
            }
            yield break;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            parent.Map.GetComponent<Tenants_MapComponent>().FindNoticeBoardInMap();
        }
    }
    public class CompProps_NoticeBoard : CompProperties {
        public CompProps_NoticeBoard() {
            compClass = typeof(NoticeBoard_Component);
        }
    }
}
