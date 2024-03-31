using System;
using RimWorld;
using RimWorld.QuestGen;
using Tenants.Models;
using Verse;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
namespace Tenants.QuestNodes
{
	public class QuestNode_ContractConclusion : QuestNode
	{
		public SlateRef<Map> map;
		public SlateRef<Contract> contract;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> inSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> outSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> badSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> joinSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> leaveSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> recruitSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> rejectSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> terminateSignal;

		protected override void RunInt()
		{
			try
			{
				Slate slate = QuestGen.slate;
				Quest quest = QuestGen.quest;
				quest.DescriptionPart("[questDescriptionBeforeAccepted]", quest.AddedSignal, null,
					QuestPart.SignalListenMode.Always);

				quest.DescriptionPart("[questDescriptionAfterAccepted]", null, null,
					QuestPart.SignalListenMode.Always);
				
				map.TryGetValue(slate, out Map colonyMap);
				contract.TryGetValue(slate, out Contract cont);
				var payment = new QuestPart_ContractConclusion
				{
					inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
					outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate)),
					badSignal = QuestGenUtility.HardcodedSignalWithQuestID(badSignal.GetValue(slate)),
					joinSignal = QuestGenUtility.HardcodedSignalWithQuestID(joinSignal.GetValue(slate)),
					leaveSignal = QuestGenUtility.HardcodedSignalWithQuestID(leaveSignal.GetValue(slate)),
					recruitSignal = QuestGenUtility.HardcodedSignalWithQuestID(recruitSignal.GetValue(slate)),
					rejectSignal = QuestGenUtility.HardcodedSignalWithQuestID(rejectSignal.GetValue(slate)),
					terminateSignal = QuestGenUtility.HardcodedSignalWithQuestID(terminateSignal.GetValue(slate)),
					signalListenMode = QuestPart.SignalListenMode.Always,
					contract = cont,
					map = colonyMap
				};

				quest.AddPart(payment);
			}
			catch (Exception ex)
			{
				Log.Message("Error at QuestNode_ContractConclusion RunInt: " + ex.Message);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			map.TryGetValue(slate, out Map colonyMap);
			return colonyMap != null;

		}
	}
}