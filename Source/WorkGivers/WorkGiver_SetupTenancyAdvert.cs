using RimWorld;
using System.Collections.Generic;
using Tenants.Things;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.WorkGivers
{
	public class WorkGiver_SetupTenancyAdvert : WorkGiver_Scanner
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			Danger maxDanger = pawn.NormalMaxDanger();
			List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
			foreach (Building t in bList)
			{
				if (t is NoticeBoard buildingNoticeBoard &&
				    buildingNoticeBoard._isActive &&
				    !buildingNoticeBoard._noticeUp &&
				    pawn.CanReach(buildingNoticeBoard, PathEndMode.InteractionCell, maxDanger) &&
				    !buildingNoticeBoard.IsBurning())
				{
					yield return t;
				}
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.GetLord() != null || base.ShouldSkip(pawn, forced);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!t.Spawned)
			{
				return false;
			}

			return !(t is NoticeBoard) || CanAddNotice(pawn, t);
		}

		private static bool CanAddNotice(Pawn pawn, Thing t)
		{
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null))
			{
				return false;
			}

			if (t.Faction != pawn.Faction)
			{
				return false;
			}

			if (!(t is NoticeBoard buildingNoticeBoard))
			{
				return false;
			}

			if (buildingNoticeBoard._noticeUp || !buildingNoticeBoard._isActive)
			{
				return false;
			}

			var thingDefCountClass =
				new ThingDefCountClass(ThingDefOf.Silver, buildingNoticeBoard.AdvertisementCost());
			return pawn.Map.itemAvailability.ThingsAvailableAnywhere(thingDefCountClass, pawn);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(Defs.JobDefOf.LTS_AddNotice, t);
			job.GetTargetQueue(TargetIndex.B);
			bool needMore = true;
			if (!((ThingWithComps)job.GetTarget(TargetIndex.A).Thing is NoticeBoard noticeBoard))
			{
				Log.Message("ERROR TEST TENANTS");
				return null;
			}
			job.count = noticeBoard.AdvertisementCost();
			int counter = 0;
			var thingList = new List<Thing>();
			Thing temp = null;
			while (needMore)
			{
				bool Validator(Thing pay)
				{
					if (!pay.Spawned)
					{
						return false;
					}

					if (pay.IsForbidden(pawn))
					{
						return false;
					}

					if (!pawn.CanReserve(pay))
					{
						return false;
					}

					return !thingList.Contains(pay);
				}

				Thing thing = GenClosest.ClosestThing_Global_Reachable(temp?.Position ?? pawn.Position,
					pawn.Map, pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Silver), PathEndMode.OnCell,
					TraverseParms.For(pawn), 9999f, Validator);

				if (thing == null)
				{
					break;
				}

				job.targetQueueB.Add(thing);
				thingList.Add(thing);
				counter += thing.stackCount;
				temp = thing;
				if (counter >= job.count)
				{
					needMore = false;
				}
			}

			return job;
		}
	}
}