﻿using RimWorld;
using System;
using Tenants.Components;
using Verse;

namespace Tenants.ThoughtWorkers {
    public class ThoughtWorker_PreceptTenancy : ThoughtWorker_Precept {
        protected override ThoughtState ShouldHaveThought(Pawn p) {
            if (!ModsConfig.IdeologyActive) {
                return ThoughtState.Inactive;
            }
            Tenants_MapComponent comp = p.Map.GetComponent<Tenants_MapComponent>();
            return comp != null ? ThoughtState.ActiveAtStage(comp.ActiveContracts.Count) : false;
        }

    }
}
