using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Tenants.Components;
using Tenants.Language;
using Tenants.Letters;
using Tenants.Logic;
using Tenants.Models;
using UnityEngine;
using Verse;
using HistoryEventDefOf = Tenants.Defs.HistoryEventDefOf;
using ThoughtDefOf = Tenants.Defs.ThoughtDefOf;

// ReSharper disable InconsistentNaming

namespace Tenants.QuestNodes
{
	public class QuestPart_ContractConclusion : QuestPart
	{
		public string inSignal; //Contract Finished
		public string outSignal; //Contract Reboot
		public string terminateSignal; //Signal out if player reject further tenancy
		public string badSignal; //When tenant dies
		public string joinSignal; //When tenant joins
		public string joinAcceptSignal; //When tenant is accepted to join
		public string joinRejectSignal; //When tenant is rejected to join
		public string recruitSignal; //When tenant is recruited
		public string rejectSignal; //When tenant decides against coming
		public string leaveSignal; //When tenant decides against coming

		private bool isEnded; //Makes sure the kill penalty is not applied many times.
		private bool askedJoin;
		public Map map;
		public Contract contract;

		private QuestPart_GenerateTenant _cachedGenerateTenantQuestPart;

		private QuestPart_GenerateTenant _generateTenantQuestPart =>
			_cachedGenerateTenantQuestPart
			?? (_cachedGenerateTenantQuestPart = this.quest.GetFirstPartOfType<QuestPart_GenerateTenant>());

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (Settings.Settings.DebugLog)
			{
				Log.Message("Notify_QuestSignalReceived: " + signal);
			}

			if (signal.tag == inSignal)
			{
				if (_generateTenantQuestPart.AutoRenewal)
				{
					Messages.Message(Translate.AutoRenewContractText(contract?._tenant), MessageTypeDefOf.NeutralEvent);
					Find.SignalManager.SendSignal(new Signal(outSignal));
					return;
				}

				TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
				comp.Payday(contract);
				var diaNode =
					new DiaNode(Translate.ContractText(contract._tenant, contract._rent, contract.LengthDays));

				var agree = new DiaOption(Translate.ContractAgree)
				{
					action = delegate
					{
						Find.SignalManager.SendSignal(new Signal(outSignal));
					},
					resolveTree = true
				};

				var reject = new DiaOption(Translate.ContractReject)
				{
					action = delegate
					{
						Find.SignalManager.SendSignal(new Signal(terminateSignal));
					},
					resolveTree = true
				};

				diaNode.options.Add(agree);
				diaNode.options.Add(reject);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true,
					Translate.ContractTitle));
			}
			else if (contract._mayJoin && signal.tag == joinSignal && !askedJoin)
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(Translate.ContractJoin(contract._tenant));
				stringBuilder.AppendLine();

				TenancyLogic.AppendWorkTypes(stringBuilder, "IncapableOfTooltipWorkTypes".Translate(),
					contract._tenant.story.DisabledWorkTagsBackstoryAndTraits);

				TenancyLogic.AppendListData(stringBuilder, "Traits".Translate(),
					contract._tenant.story.traits.allTraits, t => t.LabelCap);

				var minorPassion = new List<SkillRecord>();
				var majorPassion = new List<SkillRecord>();
				var otherPassions = new List<SkillRecord>();
				foreach (SkillRecord t in contract._tenant.skills.skills)
				{
					switch (t.passion)
					{
						case Passion.None:
							break;

						case Passion.Minor:
							minorPassion.Add(t);
							break;

						case Passion.Major:
							majorPassion.Add(t);
							break;

						default:
							otherPassions.Add(t);
							break;
					}
				}

				TenancyLogic.AppendListData(stringBuilder, Translate.TenantPassionMinor, minorPassion,
					p => p.def.skillLabel.CapitalizeFirst());
				TenancyLogic.AppendListData(stringBuilder, Translate.TenantPassionMajor, majorPassion,
					p => p.def.skillLabel.CapitalizeFirst());

				if (otherPassions.Any())
				{
					TenancyLogic.AppendListData(stringBuilder, Translate.TenantPassionOther, otherPassions,
						p => $"{p.def.skillLabel.CapitalizeFirst()} ({p.passion.GetLabel()})");
				}

				var choiceLetter = (TenancyJoinLetter)LetterMaker.MakeLetter(Translate.ContractTitle,
					stringBuilder.ToString(), Defs.LetterDefs.LTS_TenancyJoinLetter,
					new LookTargets(contract._tenant), null, this.quest);
				choiceLetter.signalAccept = joinAcceptSignal ?? recruitSignal;
				choiceLetter.signalReject = joinRejectSignal;
				choiceLetter.radioMode = true;
				choiceLetter.StartTimeout(10000);
				Find.LetterStack.ReceiveLetter(choiceLetter);
			}
			else if (signal.tag == this.joinRejectSignal)
			{
				Messages.Message(Translate.ContractJoinReject(contract._tenant), MessageTypeDefOf.NeutralEvent);
				contract._tenant.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.LTS_JoinRejection);
				askedJoin = true;
			}
			else if (signal.tag == this.joinAcceptSignal)
			{
				TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
				comp.Payday(contract);
				Messages.Message(Translate.ContractJoinAccept(contract._tenant),
					MessageTypeDefOf.PositiveEvent);

				Find.SignalManager.SendSignal(new Signal(recruitSignal));
				contract._tenant.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.LTS_JoinAccept);
				contract._tenant.SetFaction(Faction.OfPlayerSilentFail);
				contract._tenant.apparel.UnlockAll();
				Find.HistoryEventsManager.RecordEvent(
					new HistoryEvent(HistoryEventDefOf.TenancyJoin,
						contract._tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
			}
			else if (signal.tag == badSignal && contract._tenant.Spawned && !isEnded)
			{
				TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
				if (Settings.Settings.KillPenalty)
				{
					comp.TenantKills++;
					if (Settings.Settings.DebugLog)
					{
						Log.Message("Signal Dead Tenants: " + comp.TenantKills);
					}
				}

				Find.HistoryEventsManager.RecordEvent(
					new HistoryEvent(HistoryEventDefOf.TenancyDeath,
						contract._tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);

				isEnded = true;
			}
			else if (signal.tag == rejectSignal)
			{
				TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
				comp.TenantKills--;
			}
			else if (signal.tag == leaveSignal)
			{
				Find.HistoryEventsManager.RecordEvent(
					new HistoryEvent(HistoryEventDefOf.TenancyLeave,
						contract._tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "InSignal");
			Scribe_Values.Look(ref outSignal, "OutSignal");
			Scribe_Values.Look(ref terminateSignal, "TerminateSignal");
			Scribe_Values.Look(ref badSignal, "BadSignal");
			Scribe_Values.Look(ref joinSignal, "JoinSignal");
			Scribe_Values.Look(ref joinRejectSignal, "joinRejectSignal");
			Scribe_Values.Look(ref joinAcceptSignal, "joinAcceptSignal");
			Scribe_Values.Look(ref recruitSignal, "RecruitSignal");
			Scribe_Values.Look(ref rejectSignal, "RejectSignal");
			Scribe_Values.Look(ref leaveSignal, "LeaveSignal");
			Scribe_Values.Look(ref isEnded, "IsEnded");
			Scribe_Deep.Look(ref contract, "Contract");
			Scribe_References.Look(ref map, "Map");
		}
	}
}