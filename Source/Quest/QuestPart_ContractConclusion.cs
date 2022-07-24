using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestPart_ContractConclusion : QuestPart {
        public string inSignal;
        public string outSignal;
        public string terminateSignal;
        public string joinSignal;
        public string recruitSignal;
        public Map map;
        public Models.Contract contract;
        public override void Notify_QuestSignalReceived(Signal signal) {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == this.inSignal) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.Payday(contract);
                DiaNode diaNode = new DiaNode(Language.Translate.ContractText(contract.tenant, contract.rent, contract.LengthDays));
                DiaOption agree = new DiaOption(Language.Translate.ContractAgree) {
                    action = delegate {
                        Find.SignalManager.SendSignal(new Signal(this.outSignal));
                    }
                };
                agree.resolveTree = true;
                DiaOption reject = new DiaOption(Language.Translate.ContractReject) {
                    action = delegate {
                        Find.SignalManager.SendSignal(new Signal(this.terminateSignal));
                    }
                };
                reject.resolveTree = true;
                diaNode.options.Add(agree);
                diaNode.options.Add(reject);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, Language.Translate.ContractTitle));
            }
            else if (signal.tag == this.joinSignal) {
                DiaNode diaNode = new DiaNode(Language.Translate.ContractJoin(contract.tenant));
                DiaOption agree = new DiaOption(Language.Translate.ContractAgree) {
                    action = delegate {
                        Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                        comp.Payday(contract);
                        Messages.Message(Language.Translate.ContractJoinAccept(contract.tenant), MessageTypeDefOf.PositiveEvent);
                        contract.tenant.needs.mood.thoughts.memories.TryGainMemory(Defs.ThoughtDefOf.LTS_JoinAccept, null, null);
                        Find.SignalManager.SendSignal(new Signal(this.recruitSignal));
                        contract.tenant.SetFaction(Faction.OfPlayerSilentFail);
                        contract.tenant.apparel.UnlockAll();
                    }
                };
                agree.resolveTree = true;
                DiaOption reject = new DiaOption(Language.Translate.ContractReject) {
                    action = delegate {
                        Messages.Message(Language.Translate.ContractJoinReject(contract.tenant), MessageTypeDefOf.NeutralEvent);
                        contract.tenant.needs.mood.thoughts.memories.TryGainMemory(Defs.ThoughtDefOf.LTS_JoinRejection, null, null);
                    }
                };
                reject.resolveTree = true;
                diaNode.options.Add(agree);
                diaNode.options.Add(reject);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, Language.Translate.ContractTitle));
            }
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "InSignal", null, false);
            Scribe_Values.Look(ref outSignal, "OutSignal", null, false);
            Scribe_Values.Look(ref terminateSignal, "TerminateSignal", null, false);
            Scribe_Values.Look(ref joinSignal, "JoinSignal", null, false);
            Scribe_Values.Look(ref recruitSignal, "RecruitSignal", null, false);
            Scribe_Deep.Look(ref contract, "Contract");
            Scribe_References.Look(ref map, "Map");
        }
    }
}
