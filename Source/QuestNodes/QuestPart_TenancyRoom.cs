using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Tenants.Language;
using Tenants.Models;
using Verse;

// ReSharper disable InconsistentNaming

namespace Tenants.QuestNodes
{
	public class QuestPart_TenancyRoom : QuestPartActivable
	{
		public Contract contract;
		private readonly List<Pawn> culpritsResult = new List<Pawn>();
		private bool showAlert = true;
		public int withoutRoomTicks;
		public int maxTicks;
		private string OutSignalFailed => "Quest" + this.quest.id + ".Part" + this.Index + ".Failed";
		public List<string> outSignalsFailed = new List<string>();

		public override AlertReport AlertReport
		{
			get
			{
				if (!showAlert || withoutRoomTicks < 60)
				{
					return AlertReport.Inactive;
				}

				if (contract?._tenant == null)
				{
					return AlertReport.Inactive;
				}

				culpritsResult.Clear();
				if (!HaveAndRequireRoomFulfilled())
				{
					culpritsResult.Add(contract._tenant);
				}

				return AlertReport.CulpritsAre(culpritsResult);
			}
		}
		
		public override bool AlertCritical => true;
		public override string AlertLabel => Translate.TenancyRoomRequired;
		public override string AlertExplanation => Translate.TenancyRoomRequiredDesc;
		
		public override void QuestPartTick()
		{
			base.QuestPartTick();
			if (contract?._tenant == null)
			{
				return;
			}
			
			if (HaveAndRequireRoomFulfilled())
			{
				withoutRoomTicks = 0;
				return;
			}

			withoutRoomTicks++;
			if (withoutRoomTicks < maxTicks)
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref contract, "Contract");
			Scribe_Collections.Look(ref outSignalsFailed, "OutSignalsFailed");
			Scribe_Values.Look(ref showAlert, "ShowAlert", defaultValue: true);
			Scribe_Values.Look(ref withoutRoomTicks, "WithoutRoomTicks");
			Scribe_Values.Look(ref maxTicks, "MaxTicks");
		}

		public bool HaveAndRequireRoomFulfilled()
		{
			if (contract._tenant.royalty.HasPersonalBedroom())
			{
				return true;
			}
			return !contract._singleRoomRequirement;
		}
	}

}