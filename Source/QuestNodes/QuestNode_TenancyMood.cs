using System;
using JetBrains.Annotations;
using RimWorld.QuestGen;
using Tenants.Models;
using Verse;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace Tenants.QuestNodes {
    public class QuestNode_TenancyMood : QuestNode {
        [NoTranslate]
        public SlateRef<string> inSignalEnable;
        [NoTranslate]
        public SlateRef<string> outSignal;
        public SlateRef<Contract> contract;
        public SlateRef<float> thresholdLow;
        public SlateRef<float> thresholdHigh;
        [UsedImplicitly] 
        public QuestNode node;
        [UsedImplicitly] 
        public QuestNode elseNode;
        protected override void RunInt() {
            try {
                Slate slate = QuestGen.slate;
                if (contract.GetValue(slate) == null)
                {
	                return;
                }

                var questPartTenancyMoodAbove = new QuestPart_TenancyMood {
	                contract = contract.GetValue(slate),
	                thresholdLow = thresholdLow.GetValue(slate),
	                thresholdHigh = thresholdHigh.GetValue(slate),
	                minTicksBelowThreshold = Settings.Settings.MoodTicks,
	                minTicksAboveThreshold = Settings.Settings.MoodTicks,
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
                if (elseNode != null) {
	                string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
	                questPartTenancyMoodAbove.outSignalsCompleted.Add(text);
	                QuestGenUtility.RunInnerNode(elseNode, text);
                }
                QuestGen.quest.AddPart(questPartTenancyMoodAbove);
            }
            catch (Exception ex) {
	            Log.Error($"LTS_Tenants Error - QuestNode_TenancyMood: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override bool TestRunInt(Slate slate) {
            bool result = true;
            if (node != null) {
                result = node.TestRun(slate);
            }
            if (elseNode != null) {
                result = result && elseNode.TestRun(slate);
            }
            return result;
        }
    }
}
