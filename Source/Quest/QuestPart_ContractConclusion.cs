using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestPart_ContractConclusion : QuestPart {
        public string inSignal;             //Contract Finished
        public string outSignal;            //Contract Reboot
        public string terminateSignal;      //Signal out if player reject further tenancy
        public string badSignal;            //When tenant dies
        public string joinSignal;           //When tenant joins
        public string recruitSignal;        //When tenant is recruited
        public string rejectSignal;         //When tenant decides against coming
        public string initiateSignal;       //When quest is accepted
        public bool isEnded = false;        //Makes sure the kill penalty is not applied many times.
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
                        comp.ActiveContracts.Remove(contract);
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
                diaNode.options.Add(agree);
                diaNode.options.Add(reject);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, Language.Translate.ContractTitle));
            }
            else if (signal.tag == this.badSignal && contract.tenant.Spawned && !isEnded) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.ActiveContracts.Remove(contract);
                comp.TenantKills++;
                isEnded = true;
            }
            else if (signal.tag == this.rejectSignal) {
                Components.Tenants_MapComponent comp = map.GetComponent<Components.Tenants_MapComponent>();
                comp.TenantKills--;
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
            Scribe_Values.Look(ref isEnded, "IsEnded", false, false);
            Scribe_Deep.Look(ref contract, "Contract");
            Scribe_References.Look(ref map, "Map");
        }
    }
}
