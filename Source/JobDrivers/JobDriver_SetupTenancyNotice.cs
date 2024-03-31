using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Tenants.Things;

namespace Tenants.JobDrivers
{
	internal class JobDriver_SetupTenancyNotice : JobDriver
	{
		private NoticeBoard NoticeBoard => (ThingWithComps)this.job.GetTarget(TargetIndex.A).Thing as NoticeBoard;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job);
			this.pawn.Reserve(NoticeBoard, this.job, 1, -1, null, errorOnFailed);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil rest = CheckForRest(TargetIndex.B);
			yield return rest;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true);
			yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, rest);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
				.FailOnDespawnedNullOrForbidden(TargetIndex.A);

			yield return Toils_General.Wait(240)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
				.WithProgressBarToilDelay(TargetIndex.A);

			yield return new Toil
			{
				initAction = delegate
				{
					int amount = NoticeBoard.AdvertisementCost();
					if (this.pawn.carryTracker.CarriedThing == null ||
					    amount < this.pawn.carryTracker.CarriedThing.stackCount)
					{
						Messages.Message(Language.Translate.AdvertisementFailed(this.pawn), NoticeBoard,
							MessageTypeDefOf.NeutralEvent);

						this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out _);
					}
					else
					{
						NoticeBoard._noticeUp = true;
						NoticeBoard._silverAmount += amount;
						this.pawn.carryTracker.CarriedThing.SplitOff(amount).Destroy();
						if (Settings.Settings.AdvertNoticeSound)
						{
							Messages.Message(Language.Translate.AdvertisementPlaced(), NoticeBoard,
								MessageTypeDefOf.NeutralEvent);
						}
					}
				},
			};

			yield break;
		}

		private static Toil CheckForRest( /*Toil nextToil,*/ TargetIndex haulableInd)
		{
			var toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(haulableInd);
				if (targetQueue.NullOrEmpty())
				{
					return;
				}

				curJob.SetTarget(haulableInd, targetQueue[0]);
				targetQueue.RemoveAt(0);
			};

			return toil;
		}
	}
}