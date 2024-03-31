using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Tenants.Language;
using Tenants.Models;
using Verse;

// ReSharper disable InconsistentNaming

namespace Tenants.QuestNodes
{
	public class QuestPart_TenancyMood : QuestPartActivable
	{
		public Contract contract;
		private readonly List<Pawn> culpritsResult = new List<Pawn>();
		private int moodBelowThresholdTicks;
		private int moodAboveThresholdTicks;
		public int minTicksBelowThreshold;
		public int minTicksAboveThreshold;
		private bool offeredJoin;
		private string OutSignalFailed => "Quest" + this.quest.id + ".Part" + this.Index + ".Failed";
		public List<string> outSignalsFailed = new List<string>();
		private bool showAlert = true;
		public float thresholdLow;
		public float thresholdHigh;

		public override AlertReport AlertReport
		{
			get
			{
				if (!showAlert || minTicksBelowThreshold < 60)
				{
					return AlertReport.Inactive;
				}

				if (contract?._tenant == null)
				{
					return AlertReport.Inactive;
				}

				culpritsResult.Clear();
				if (MoodBelowThreshold(contract._tenant))
				{
					culpritsResult.Add(contract._tenant);
				}

				return AlertReport.CulpritsAre(culpritsResult);
			}
		}


		public override bool AlertCritical => true;

		public override string AlertLabel => Translate.MoodBelowThreshold();

		public override string AlertExplanation => Translate.MoodBelowThresholdDesc(contract._tenant);

		public override void QuestPartTick()
		{
			base.QuestPartTick();
			if (contract == null)
			{
				return;
			}
			if (MoodBelowThreshold(contract._tenant))
			{
				moodBelowThresholdTicks++;
				if (moodBelowThresholdTicks < minTicksBelowThreshold)
				{
					return;
				}
				
				var signalArgs = new SignalArgs(contract._tenant.Named("SUBJECT"));
				Find.SignalManager.SendSignal(new Signal(OutSignalFailed, signalArgs));
				foreach (string t in outSignalsFailed.Where(t => !t.NullOrEmpty()))
				{
					Find.SignalManager.SendSignal(new Signal(t, signalArgs));
				}
			}
			else if (MoodAboveThreshold(contract._tenant))
			{
				moodAboveThresholdTicks++;
				if (moodAboveThresholdTicks < minTicksAboveThreshold)
				{
					return;
				}

				moodAboveThresholdTicks = 0;
				var signalArgs = new SignalArgs(contract._tenant.Named("SUBJECT"));
				Find.SignalManager.SendSignal(new Signal(this.OutSignalCompleted, signalArgs));
				foreach (string t in this.outSignalsCompleted.Where(t => !t.NullOrEmpty()))
				{
					Find.SignalManager.SendSignal(new Signal(t, signalArgs));
				}
			}
			else
			{
				moodAboveThresholdTicks = 0;
				moodBelowThresholdTicks = 0;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref contract, "Contract");
			Scribe_Values.Look(ref moodBelowThresholdTicks, "MoodBelowThresholdTicks");
			Scribe_Values.Look(ref moodAboveThresholdTicks, "MoodAboveThresholdTicks");
			Scribe_Values.Look(ref minTicksBelowThreshold, "MinTicksBelowThreshold");
			Scribe_Values.Look(ref minTicksAboveThreshold, "MinTicksAboveThreshold");
			Scribe_Values.Look(ref offeredJoin, "OfferedJoin", defaultValue: false);
			Scribe_Collections.Look(ref outSignalsFailed, "OutSignalsFailed");
			Scribe_Values.Look(ref showAlert, "ShowAlert", defaultValue: true);
			Scribe_Values.Look(ref thresholdLow, "ThresholdLow");
			Scribe_Values.Look(ref thresholdHigh, "ThresholdHigh");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			if (Find.AnyPlayerHomeMap == null)
			{
				return;
			}

			Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
			contract._tenant = randomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault();
			thresholdLow = 0.25f;
			minTicksBelowThreshold = 2500;
		}

		private bool MoodBelowThreshold(Pawn pawn)
		{
			if (pawn.needs?.mood == null)
			{
				return false;
			}

			return pawn.needs.mood.CurLevelPercentage < thresholdLow;
		}

		private bool MoodAboveThreshold(Pawn pawn)
		{
			if (pawn.needs?.mood == null)
			{
				return false;
			}

			return pawn.needs.mood.CurLevelPercentage > thresholdHigh;
		}
	}
}