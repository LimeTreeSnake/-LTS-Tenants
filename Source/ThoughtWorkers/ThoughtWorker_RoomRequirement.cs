﻿using RimWorld;
using System;
using System.Collections.Generic;
using Tenants.Components;
using Verse;

namespace Tenants.ThoughtWorkers {
    public class ThoughtWorker_RoomRequirement : ThoughtWorker {

        protected override ThoughtState CurrentStateInternal(Pawn p) {
            if (p.MapHeld == null || !p.MapHeld.IsPlayerHome) {
                return false;
            }
            Tenants_MapComponent comp = p.Map.GetComponent<Tenants_MapComponent>();
            if (comp.IsContractedTenant(p, out Models.Contract cont)) {
                if (cont.singleRoomRequirement) {
                    return !p.royalty.HasPersonalBedroom();
                }
            }
            return false;
        }
    }
}