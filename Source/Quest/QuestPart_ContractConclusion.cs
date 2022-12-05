using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Verse.Dialog_InfoCard;

namespace Tenants.QuestNodes {
    public class QuestPart_ContractConclusion : QuestPart {
        public string inSignal;             //Contract Finished
        public string outSignal;            //Contract Reboot
        public string terminateSignal;      //Signal out if player reject further tenancy
        public string badSignal;            //When tenant dies
        public string joinSignal;           //When tenant joins
        public string recruitSignal;        //When tenant is recruited
        public string rejectSignal;         //When tenant decides against coming
        public string leaveSignal;         //When tenant decides against coming
        public string initiateSignal;       //When quest is accepted
        public string postponeSignal;       //If pawn join offer is postponed
        public bool isEnded = false;        //Makes sure the kill penalty is not applied many times.
        public Map map;
        public Models.Contract contract;

        public override void Notify_PreCleanup() {
            base.Notify_PreCleanup();
            if (this.quest.State == QuestState.EndedOfferExpired) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.RemoveTenant(contract.tenant);
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal) {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == this.inSignal) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.Payday(contract);
                DiaNode diaNode = new DiaNode(Language.Translate.ContractText(contract.tenant, contract.rent, contract.LengthDays));
                DiaOption agree = new DiaOption(Language.Translate.ContractAgree) {
                    action = delegate {
                        Find.SignalManager.SendSignal(new Signal(this.outSignal));
                    },
                    resolveTree = true
                };
                DiaOption reject = new DiaOption(Language.Translate.ContractReject) {
                    action = delegate {
                        comp.ActiveContracts.Remove(contract);
                        Find.SignalManager.SendSignal(new Signal(this.terminateSignal));
                    },
                    resolveTree = true
                };
                diaNode.options.Add(agree);
                diaNode.options.Add(reject);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, Language.Translate.ContractTitle));
            }
            else if (signal.tag == this.initiateSignal) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.ActiveContracts.Add(contract);
            }
            else if (signal.tag == this.joinSignal) {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Language.Translate.ContractJoin(contract.tenant));
                List<SkillRecord> minorPassion = new List<SkillRecord>();
                List<SkillRecord> majorPassion = new List<SkillRecord>();
                WorkTags tag = contract.tenant.story.DisabledWorkTagsBackstoryAndTraits;
                for (int i = 0; i < contract.tenant.skills.skills.Count; i++) {
                    switch (contract.tenant.skills.skills[i].passion) {
                        case Passion.None:
                            break;
                        case Passion.Minor:
                            minorPassion.Add(contract.tenant.skills.skills[i]);
                            break;
                        case Passion.Major:
                            majorPassion.Add(contract.tenant.skills.skills[i]);
                            break;
                        default:
                            break;
                    }
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendInNewLine("IncapableOfTooltipWorkTypes".Translate().Colorize(ColoredText.NameColor));
                stringBuilder.AppendLine();
                stringBuilder.Append("   ");
                if (tag > WorkTags.None) {
                    foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs) {
                        if ((allDef.workTags & tag) > WorkTags.None) {
                            stringBuilder.Append(" - ");
                            stringBuilder.Append(allDef.pawnLabel);
                        }
                    }
                }
                else {
                    stringBuilder.Append(" - ");
                }
                stringBuilder.AppendInNewLine(("Traits".Translate()).Colorize(ColoredText.NameColor));
                stringBuilder.AppendLine();
                stringBuilder.Append("   ");
                if (contract.tenant.story.traits.allTraits.Any()) {
                    for (int i = 0; i < contract.tenant.story.traits.allTraits.Count; i++) {
                        if (i >= (contract.tenant.story.traits.allTraits.Count - 1)) {
                            stringBuilder.Append(contract.tenant.story.traits.allTraits[i].LabelCap);
                        }
                        else {
                            stringBuilder.Append(contract.tenant.story.traits.allTraits[i].LabelCap + " - ");
                        }
                    }
                }
                else {
                    stringBuilder.Append(" - ");
                }
                stringBuilder.AppendInNewLine((Language.Translate.TenantPassionMinor).Colorize(ColoredText.NameColor));
                stringBuilder.AppendLine();
                stringBuilder.Append("   ");
                if (minorPassion.Any()) {
                    for (int i = 0; i < minorPassion.Count; i++) {
                        if (i >= (minorPassion.Count - 1)) {
                            stringBuilder.Append(minorPassion[i].def.skillLabel.CapitalizeFirst());
                        }
                        else {
                            stringBuilder.Append(minorPassion[i].def.skillLabel.CapitalizeFirst() + " - ");
                        }
                    }
                }
                else {
                    stringBuilder.Append(" - ");
                }
                stringBuilder.AppendInNewLine((Language.Translate.TenantPassionMajor).Colorize(ColoredText.NameColor));
                stringBuilder.AppendLine();
                stringBuilder.Append("   ");
                if (majorPassion.Any()) {
                    for (int i = 0; i < majorPassion.Count; i++) {
                        if (i >= (majorPassion.Count - 1)) {
                            stringBuilder.Append(majorPassion[i].def.skillLabel.CapitalizeFirst());
                        }
                        else {
                            stringBuilder.Append(majorPassion[i].def.skillLabel.CapitalizeFirst() + " - ");
                        }
                    }
                }
                else {
                    stringBuilder.Append(" - ");
                }
                DiaNode diaNode = new DiaNode(stringBuilder.ToString());
                DiaOption agree = new DiaOption(Language.Translate.ContractAgree) {
                    action = delegate {
                        Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                        comp.Payday(contract);
                        Messages.Message(Language.Translate.ContractJoinAccept(contract.tenant), MessageTypeDefOf.PositiveEvent);
                        contract.tenant.needs.mood.thoughts.memories.TryGainMemory(Defs.ThoughtDefOf.LTS_JoinAccept, null, null);
                        Find.SignalManager.SendSignal(new Signal(this.recruitSignal));
                        contract.tenant.SetFaction(Faction.OfPlayerSilentFail);
                        contract.tenant.apparel.UnlockAll();
                        comp.ActiveContracts.Remove(contract);
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(Defs.HistoryEventDefOf.TenancyJoin, contract.tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
                    },
                    resolveTree = true
                };
                DiaOption reject = new DiaOption(Language.Translate.ContractReject) {
                    action = delegate {
                        Messages.Message(Language.Translate.ContractJoinReject(contract.tenant), MessageTypeDefOf.NeutralEvent);
                        contract.tenant.needs.mood.thoughts.memories.TryGainMemory(Defs.ThoughtDefOf.LTS_JoinRejection, null, null);
                    },
                    resolveTree = true
                };
                DiaOption postpone = new DiaOption(Language.Translate.ContractPostpone) {
                    action = delegate {
                        Find.SignalManager.SendSignal(new Signal(this.postponeSignal));
                    },
                    resolveTree = true
                };
                diaNode.options.Add(agree);
                diaNode.options.Add(reject);
                diaNode.options.Add(postpone);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, Language.Translate.ContractTitle));
            }
            else if (signal.tag == this.badSignal && contract.tenant.Spawned && !isEnded) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.ActiveContracts.Remove(contract);
                comp.TenantKills++;
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(Defs.HistoryEventDefOf.TenancyDeath, contract.tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
                isEnded = true;
            }
            else if (signal.tag == this.rejectSignal) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.TenantKills--;
            }
            else if (signal.tag == this.leaveSignal) {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(Defs.HistoryEventDefOf.TenancyLeave, contract.tenant.Named(HistoryEventArgsNames.Quest)), canApplySelfTookThoughts: false);
            }
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "InSignal", null, false);
            Scribe_Values.Look(ref outSignal, "OutSignal", null, false);
            Scribe_Values.Look(ref terminateSignal, "TerminateSignal", null, false);
            Scribe_Values.Look(ref badSignal, "BadSignal", null, false);
            Scribe_Values.Look(ref joinSignal, "JoinSignal", null, false);
            Scribe_Values.Look(ref recruitSignal, "RecruitSignal", null, false);
            Scribe_Values.Look(ref rejectSignal, "RejectSignal", null, false);
            Scribe_Values.Look(ref leaveSignal, "LeaveSignal", null, false);
            Scribe_Values.Look(ref initiateSignal, "InitiateSignal", null, false);
            Scribe_Values.Look(ref postponeSignal, "PostponeSignal", null, false);
            Scribe_Values.Look(ref isEnded, "IsEnded", false, false);
            Scribe_Deep.Look(ref contract, "Contract");
            Scribe_References.Look(ref map, "Map");
        }
    }
}
