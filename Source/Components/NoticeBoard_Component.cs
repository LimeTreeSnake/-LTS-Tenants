using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using System;

namespace Tenants.Components {
    public class NoticeBoard_Component : ThingComp {

        public bool noticeForTenancy;

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref noticeForTenancy, "NoticeForTenancy");
        }
        public override string CompInspectStringExtra() {
            if (noticeForTenancy)
                return Language.Translate.AdvertisementPlaced;
            else
                return string.Empty;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            if (pawn.Faction.IsPlayer && pawn.RaceProps.intelligence > Intelligence.Animal && !noticeForTenancy && pawn.Map.resourceCounter.Silver > Settings.Settings.NoticeCourierCost) {
                yield return new FloatMenuOption(Language.Translate.AddNoticeForTenancy(Settings.Settings.NoticeCourierCost), delegate {
                    Job job = JobMaker.MakeJob(Defs.JobDefOf.LTS_AddNotice, this.parent);
                    job.playerForced = true;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
                });
            }
            yield break;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            parent.Map.GetComponent<Components.Tenants_MapComponent>().FindNoticeBoardInMap();
        }
    }
    public class CompProps_NoticeBoard : CompProperties {
        public CompProps_NoticeBoard() {
            compClass = typeof(NoticeBoard_Component);
        }
    }
}
