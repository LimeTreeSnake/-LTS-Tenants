using RimWorld;
using System;
using System.Collections.Generic;
using Tenants.Components;
using Verse;

namespace Tenants.ThoughtWorkers {
    public class ThoughtWorker_JoinRejection : Thought_Memory {

		public float moodOffsetOverride;
		public override int DurationTicks {
			get {
				return this.durationTicksOverride;
			}
		}
		public override float MoodOffset() {
			return this.moodOffsetOverride;
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.moodOffsetOverride, "moodOffsetOverride", 0f, false);
		}
	}
}
