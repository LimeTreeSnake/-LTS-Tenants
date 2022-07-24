using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace Tenants.QuestNodes {
    class QuestNode_ContractConclusion : QuestNode {
        public SlateRef<Map> map;
        public SlateRef<Models.Contract> contract;
        [NoTranslate, TranslationHandle(Priority = 100)]
        public SlateRef<string> inSignal;
        [NoTranslate, TranslationHandle(Priority = 100)]
        public SlateRef<string> outSignal;
        [NoTranslate, TranslationHandle(Priority = 100)]
        public SlateRef<string> terminateSignal;
        [NoTranslate, TranslationHandle(Priority = 100)]
        public SlateRef<string> joinSignal;
        [NoTranslate, TranslationHandle(Priority = 100)]
        public SlateRef<string> recruitSignal;
        public SlateRef<QuestPart.SignalListenMode?> signalListenMode;
        protected override void RunInt() {
            try {
                Slate slate = QuestGen.slate;
                map.TryGetValue(slate, out Map colonyMap);
                contract.TryGetValue(slate, out Models.Contract cont);
                QuestPart_ContractConclusion payment = new QuestPart_ContractConclusion {
                    inSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.inSignal.GetValue(slate)),
                    outSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.outSignal.GetValue(slate)),
                    terminateSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.terminateSignal.GetValue(slate)),
                    joinSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.joinSignal.GetValue(slate)),
                    recruitSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.recruitSignal.GetValue(slate)),
                    signalListenMode = (this.signalListenMode.GetValue(slate) ?? QuestPart.SignalListenMode.OngoingOnly),
                    contract = cont,
                    map = colonyMap
                };
                QuestGen.quest.AddPart(payment);
            } catch (Exception ex) {
                Log.Message("Error at QuestNode_ContractConclusion RunInt: " + ex.Message);
            }
        }

        protected override bool TestRunInt(Slate slate) {
            map.TryGetValue(slate, out Map colonyMap);
            if (colonyMap != null) {
                return true;
            }
            return false;
        }
    }
}
