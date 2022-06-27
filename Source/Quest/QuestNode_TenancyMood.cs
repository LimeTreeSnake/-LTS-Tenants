using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyMood : QuestNode {
        [NoTranslate]
        public SlateRef<string> inSignalEnable;
        [NoTranslate]
        public SlateRef<string> outSignal;
        public SlateRef<Models.Contract> contract;
        public SlateRef<float> thresholdLow;
        public SlateRef<float> thresholdHigh;
        public QuestNode node;
        public QuestNode elsenode;
        protected override void RunInt() {
            Slate slate = QuestGen.slate;
            if (contract.GetValue(slate) != null) {
                QuestPart_TenancyMood questPart_TenancyMoodAbove = new QuestPart_TenancyMood {
                    contract = contract.GetValue(slate),
                    thresholdLow = thresholdLow.GetValue(slate),
                    thresholdHigh = thresholdHigh.GetValue(slate),
                    minTicksBelowThreshold = Settings.Settings.TicksUntil,
                    minTicksAboveThreshold = Settings.Settings.TicksUntil,
                    inSignalEnable = (QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"))
                };
                if (!outSignal.GetValue(slate).NullOrEmpty()) {
                    questPart_TenancyMoodAbove.outSignalsFailed.Add(outSignal.GetValue(slate));
                    questPart_TenancyMoodAbove.outSignalsCompleted.Add(outSignal.GetValue(slate));
                }
                if (node != null) {
                    string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
                    questPart_TenancyMoodAbove.outSignalsFailed.Add(text);
                    QuestGenUtility.RunInnerNode(node, text);
                }
                if (elsenode != null) {
                    string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
                    questPart_TenancyMoodAbove.outSignalsCompleted.Add(text);
                    QuestGenUtility.RunInnerNode(elsenode, text);
                }
                QuestGen.quest.AddPart(questPart_TenancyMoodAbove);
            }
        }

        protected override bool TestRunInt(Slate slate) {
            bool result = true;
            if (node != null) {
                result = result && node.TestRun(slate);
            }
            if (elsenode != null) {
                result = result && elsenode.TestRun(slate);
            }
            return result;
        }
    }
}
