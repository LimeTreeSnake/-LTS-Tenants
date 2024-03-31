using System.Collections.Generic;
using System.Text;
using RimWorld;
using Tenants.Components;
using Tenants.Language;
using Tenants.Models;
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
		public string recruitSignal; //When tenant is recruited
		public string rejectSignal; //When tenant decides against coming
		public string leaveSignal; //When tenant decides against coming
		
		private bool isEnded; //Makes sure the kill penalty is not applied many times.
		private bool askedJoin;
		public Map map;
		public Contract contract;
		public override void Notify_QuestSignalReceived(Signal signal)
		{			
			base.Notify_QuestSignalReceived(signal);
			if (Settings.Settings.DebugLog)
			{
				Log.Message("Notify_QuestSignalReceived: " + signal);
			}
			if (signal.tag == inSignal)
			{
				TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
				comp.Payday(contract);
				var diaNode =
					new DiaNode(Translate.ContractText(contract._tenant, contract._rent, contract.LengthDays));

				var agree = new DiaOption(Translate.ContractAgree())
				{
					action = delegate
					{
						Find.SignalManager.SendSignal(new Signal(outSignal));
					},
					resolveTree = true
				};

				var reject = new DiaOption(Translate.ContractReject())
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
					Translate.ContractTitle()));
			}
			else if (signal.tag == joinSignal && !askedJoin)
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(Translate.ContractJoin(contract._tenant));
				var minorPassion = new List<SkillRecord>();
				var majorPassion = new List<SkillRecord>();
				WorkTags tag = contract._tenant.story.DisabledWorkTagsBackstoryAndTraits;
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
							Log.Message("QuestPart_ContractConclusion failed to find a passion");
							break;
					}
				}

				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendInNewLine("IncapableOfTooltipWorkTypes".Translate()
					.Colorize(ColoredText.NameColor));

				stringBuilder.AppendLine();
				stringBuilder.Append("   ");
				if (tag > WorkTags.None)
				{
					foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
					{
						if ((allDef.workTags & tag) <= WorkTags.None)
						{
							continue;
						}

						stringBuilder.Append(" - ");
						stringBuilder.Append(allDef.pawnLabel);
					}
				}
				else
				{
					stringBuilder.Append(" - ");
				}

				stringBuilder.AppendInNewLine(("Traits".Translate()).Colorize(ColoredText.NameColor));
				stringBuilder.AppendLine();
				stringBuilder.Append("   ");
				if (contract._tenant.story.traits.allTraits.Any())
				{
					for (int i = 0; i < contract._tenant.story.traits.allTraits.Count; i++)
					{
						if (i >= (contract._tenant.story.traits.allTraits.Count - 1))
						{
							stringBuilder.Append(contract._tenant.story.traits.allTraits[i].LabelCap);
						}
						else
						{
							stringBuilder.Append(contract._tenant.story.traits.allTraits[i].LabelCap + " - ");
						}
					}
				}
				else
				{
					stringBuilder.Append(" - ");
				}

				stringBuilder.AppendInNewLine((Translate.TenantPassionMinor()).Colorize(ColoredText.NameColor));
				stringBuilder.AppendLine();
				stringBuilder.Append("   ");
				if (minorPassion.Any())
				{
					for (int i = 0; i < minorPassion.Count; i++)
					{
						if (i >= (minorPassion.Count - 1))
						{
							stringBuilder.Append(minorPassion[i].def.skillLabel.CapitalizeFirst());
						}
						else
						{
							stringBuilder.Append(minorPassion[i].def.skillLabel.CapitalizeFirst() + " - ");
						}
					}
				}
				else
				{
					stringBuilder.Append(" - ");
				}

				stringBuilder.AppendInNewLine((Translate.TenantPassionMajor()).Colorize(ColoredText.NameColor));
				stringBuilder.AppendLine();
				stringBuilder.Append("   ");
				if (majorPassion.Any())
				{
					for (int i = 0; i < majorPassion.Count; i++)
					{
						if (i >= (majorPassion.Count - 1))
						{
							stringBuilder.Append(majorPassion[i].def.skillLabel.CapitalizeFirst());
						}
						else
						{
							stringBuilder.Append(majorPassion[i].def.skillLabel.CapitalizeFirst() + " - ");
						}
					}
				}
				else
				{
					stringBuilder.Append(" - ");
				}

				var diaNode = new DiaNode(stringBuilder.ToString());
				var agree = new DiaOption(Translate.ContractAgree())
				{
					action = delegate
					{
						TenantsMapComponent comp = map.GetComponent<TenantsMapComponent>();
						comp.Payday(contract);
						Messages.Message(Translate.ContractJoinAccept(contract._tenant),
							MessageTypeDefOf.PositiveEvent);

						contract._tenant.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.LTS_JoinAccept);
						Find.SignalManager.SendSignal(new Signal(recruitSignal));
						contract._tenant.SetFaction(Faction.OfPlayerSilentFail);
						contract._tenant.apparel.UnlockAll();
						Find.HistoryEventsManager.RecordEvent(
							new HistoryEvent(HistoryEventDefOf.TenancyJoin,
								contract._tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
					},
					resolveTree = true
				};

				var reject = new DiaOption(Translate.ContractReject())
				{
					action = delegate
					{
						Messages.Message(Translate.ContractJoinReject(contract._tenant), MessageTypeDefOf.NeutralEvent);
						contract._tenant.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.LTS_JoinRejection);
						askedJoin = true;
					},
					resolveTree = true
				};

				var postpone = new DiaOption(Translate.ContractPostpone())
				{
					action = delegate
					{

					},
					resolveTree = true
				};

				diaNode.options.Add(agree);
				diaNode.options.Add(reject);
				diaNode.options.Add(postpone);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true,
					Translate.ContractTitle()));
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
			Scribe_Values.Look(ref recruitSignal, "RecruitSignal");
			Scribe_Values.Look(ref rejectSignal, "RejectSignal");
			Scribe_Values.Look(ref leaveSignal, "LeaveSignal");
			Scribe_Values.Look(ref isEnded, "IsEnded");
			Scribe_Deep.Look(ref contract, "Contract");
			Scribe_References.Look(ref map, "Map");
		}
	}
}