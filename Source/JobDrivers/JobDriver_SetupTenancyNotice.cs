using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;
using System;
using Verse.Sound;

namespace Tenants.JobDrivers {
    internal class JobDriver_SetupTenancyNotice : JobDriver {
        private ThingWithComps NoticeBoard => (ThingWithComps)job.GetTarget(TargetIndex.A).Thing;
        private Components.NoticeBoard_Component comp => NoticeBoard.TryGetComp<Components.NoticeBoard_Component>();
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(NoticeBoard, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddFailCondition(() => !this.job.playerForced && comp.noticeForTenancy);
            Pawn actor = GetActor();
            this.job.count = 1;
            if (NoticeBoard != null) {
                yield return Toils.NoticeBoardToils.SetupAdvert(ThingDefOf.Silver, job);                
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1, job.count, null);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
                yield return Toils_General.Wait(240, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
                yield return Toils.NoticeBoardToils.DestroyCarriedThing();
                yield return new Toil {
                    initAction = delegate {
                        comp.noticeForTenancy = true;
                        Messages.Message(Language.Translate.AdvertisementPlaced, NoticeBoard, MessageTypeDefOf.NeutralEvent);
                    },
                };
            }
            yield break;
        }
    }
}
