using System;
using JetBrains.Annotations;
using RimWorld.QuestGen;
using Tenants.Models;
using Verse;

// ReSharper disable InconsistentNaming

namespace Tenants.QuestNodes
{
	public class QuestNode_TenancyRoom : QuestNode
	{
		public SlateRef<Contract> contract;
		[NoTranslate]
		public SlateRef<string> inSignalEnable;
		[NoTranslate]
		public SlateRef<string> outSignal;
		[UsedImplicitly] 
		public QuestNode node;

		protected override void RunInt()
		{
			try
			{
				Slate slate = QuestGen.slate;
				if (contract.GetValue(slate) == null)
				{
					return;
				}
				var questPartTenancyRoom = new QuestPart_TenancyRoom() {
					contract = contract.GetValue(slate),
					maxTicks = Settings.Settings.MoodTicks,
					inSignalEnable = (QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"))
				};
				if (!outSignal.GetValue(slate).NullOrEmpty()) {
					questPartTenancyRoom.outSignalsFailed.Add(outSignal.GetValue(slate));
				}
				if (node != null)
				{
					string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
					questPartTenancyRoom.outSignalsFailed.Add(text);
					QuestGenUtility.RunInnerNode(node, text);
				}
				QuestGen.quest.AddPart(questPartTenancyRoom);
			}
			catch (Exception ex) {
				Log.Error($"LTS_Tenants Error - QuestNode_TenancyRoom: {ex.Message}\n{ex.StackTrace}");
			}
		}
		protected override bool TestRunInt(Slate slate) {
			bool result = true;
			if (node != null) {
				result = node.TestRun(slate);
			}
			return result;
		}
	}
}