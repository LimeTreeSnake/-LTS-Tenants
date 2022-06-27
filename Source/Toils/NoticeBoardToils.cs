using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Tenants.Toils {
    internal class NoticeBoardToils {

        public static Toil SetupAdvert(ThingDef t, Job job) {
            Toil toil = new Toil();
            toil.initAction = delegate {
                Pawn actor = toil.actor;
                Predicate<Thing> validator = delegate (Thing pay) {
                    if (!pay.Spawned) {
                        return false;
                    }
                    if (pay.IsForbidden(actor)) {
                        return false;
                    }
                    if (!actor.CanReserve(pay)) {
                        return false;
                    }
                    if (pay.stackCount < Settings.Settings.NoticeCourierCost) {
                        return false;
                    }
                    return true;
                };
                job.targetB = GenClosest.ClosestThing_Global_Reachable(actor.Position, actor.Map, actor.Map.listerThings.ThingsOfDef(ThingDefOf.Silver), PathEndMode.OnCell, TraverseParms.For(actor), 9999f, validator);
                job.count = Settings.Settings.NoticeCourierCost;
                if (job.targetB == null) {
                    Messages.Message(Language.Translate.AdvertisementPlaced, null, MessageTypeDefOf.NeutralEvent);
                }
            };
            return toil;
        }
        public static Toil DestroyCarriedThing() {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.GetActor();
                if (actor.carryTracker.CarriedThing != null && !actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer, true)) {
                    Thing thing;
                    actor.carryTracker.TryDropCarriedThing(actor.Position, actor.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out thing, null);
                    thing?.Destroy();
                }
            };
            return toil;
        }

    }
}
