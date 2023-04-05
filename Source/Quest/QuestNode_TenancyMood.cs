using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyMood : QuestNode {
	    // ReSharper disable MemberCanBePrivate.Global
	    // ReSharper disable InconsistentNaming
        [NoTranslate]
        public SlateRef<string> inSignalEnable;
        [NoTranslate]
        public SlateRef<string> outSignal;
        [NoTranslate]
        public SlateRef<string> inSignalPostpone;
        public SlateRef<Models.Contract> contract;
        public SlateRef<float> thresholdLow;
        public SlateRef<float> thresholdHigh;
        public QuestNode node;
        public QuestNode elsenode;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore InconsistentNaming
        protected override void RunInt() {
            try {
                Slate slate = QuestGen.slate;
                if (contract.GetValue(slate) != null) {
                    var questPartTenancyMoodAbove = new QuestPart_TenancyMood {
                        contract = contract.GetValue(slate),
                        thresholdLow = thresholdLow.GetValue(slate),
                        thresholdHigh = thresholdHigh.GetValue(slate),
                        minTicksBelowThreshold = Settings.Settings.MoodTicks,
                        minTicksAboveThreshold = Settings.Settings.MoodTicks,
                        inSignalPostpone = inSignalPostpone.GetValue(slate),
                        inSignalEnable = (QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"))
                    };
                    if (!outSignal.GetValue(slate).NullOrEmpty()) {
                        questPartTenancyMoodAbove.outSignalsFailed.Add(outSignal.GetValue(slate));
                        questPartTenancyMoodAbove.outSignalsCompleted.Add(outSignal.GetValue(slate));
                    }
                    if (node != null) {
                        string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
                        questPartTenancyMoodAbove.outSignalsFailed.Add(text);
                        QuestGenUtility.RunInnerNode(node, text);
                    }
                    if (elsenode != null) {
                        string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
                        questPartTenancyMoodAbove.outSignalsCompleted.Add(text);
                        QuestGenUtility.RunInnerNode(elsenode, text);
                    }
                    QuestGen.quest.AddPart(questPartTenancyMoodAbove);
                }
            }
            catch (Exception ex) {
                Log.Message("Error at QuestNode_TenancyMood RunInt: " + ex.Message);
            }
        }

        protected override bool TestRunInt(Slate slate) {
            bool result = true;
            if (node != null) {
                result = node.TestRun(slate);
            }
            if (elsenode != null) {
                result = result && elsenode.TestRun(slate);
            }
            return result;
        }
    }
}
