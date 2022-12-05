using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;
using System;
using Verse.Sound;
using System.Net.NetworkInformation;
using Tenants.Things;

namespace Tenants.JobDrivers {
    internal class JobDriver_SetupTenancyNotice : JobDriver {
        private NoticeBoard noticeBoard => (ThingWithComps)job.GetTarget(TargetIndex.A).Thing as NoticeBoard;
        private int paid = 0;

        public const TargetIndex IngredientInd = TargetIndex.B;
        public const TargetIndex CarriedThing = TargetIndex.C;
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            pawn.Reserve(noticeBoard, job, 1, -1, null, errorOnFailed);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            AddFailCondition(() => !this.job.playerForced);
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
                        Messages.Message(Language.Translate.AdvertisementFailed(pawn), noticeBoard, MessageTypeDefOf.NeutralEvent);
                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                    }
                    else {
                        noticeBoard.noticeUp = true;
                        if (pawn.carryTracker.CarriedThing != null && pawn.carryTracker.TryDropCarriedThing(pawn.Position, pawn.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out Thing thing, null)) {
                            thing?.Destroy();
                        }
                        if (Settings.Settings.AdvertNoticeSound) {
                            Messages.Message(Language.Translate.AdvertisementPlaced, noticeBoard, MessageTypeDefOf.NeutralEvent);
                        }
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
