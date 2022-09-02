using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;
using System;
using Verse.Sound;
using System.Net.NetworkInformation;

namespace Tenants.JobDrivers {
    internal class JobDriver_SetupTenancyNotice : JobDriver {
        private ThingWithComps NoticeBoard => (ThingWithComps)job.GetTarget(TargetIndex.A).Thing;
        private int paid = 0;

        public const TargetIndex IngredientInd = TargetIndex.B;
        public const TargetIndex CarriedThing = TargetIndex.C;
        private Components.NoticeBoard_Component comp => NoticeBoard.TryGetComp<Components.NoticeBoard_Component>();
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            pawn.Reserve(NoticeBoard, job, 1, -1, null, errorOnFailed);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            AddFailCondition(() => !this.job.playerForced && comp.noticeForTenancy);
            Toil rest = CheckForRest(TargetIndex.B);
            yield return rest;
            Toil go = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return go;
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false);
            yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, rest);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_General.Wait(240, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil {
                initAction = delegate {
                    if (job.GetTarget(TargetIndex.B).Thing.stackCount < Settings.Settings.NoticeCourierCost) {
                        Messages.Message(Language.Translate.AdvertisementFailed(pawn), NoticeBoard, MessageTypeDefOf.NeutralEvent);
                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                    }
                    else {
                        comp.noticeForTenancy = true;
                        if (pawn.carryTracker.CarriedThing != null && !pawn.carryTracker.innerContainer.TryTransferToContainer(pawn.carryTracker.CarriedThing, pawn.inventory.innerContainer, true)) {
                            pawn.carryTracker.TryDropCarriedThing(pawn.Position, pawn.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out Thing thing, null);
                            thing?.Destroy();
                        }
                        Messages.Message(Language.Translate.AdvertisementPlaced, NoticeBoard, MessageTypeDefOf.NeutralEvent);
                    }
                },
            };
            yield break;
        }
        public static Toil CheckForRest(/*Toil nextToil,*/ TargetIndex haulableInd) {
            Toil toil = new Toil();
            toil.initAction = delegate {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(haulableInd);
                if (!targetQueue.NullOrEmpty()) {
                    curJob.SetTarget(haulableInd, targetQueue[0]);
                    targetQueue.RemoveAt(0);
                }
            };
            return toil;
        }
    }
}
