using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace Tenants.QuestNodes
{
	class QuestNode_ContractConclusion : QuestNode
	{
		// ReSharper disable MemberCanBePrivate.Global
		// ReSharper disable InconsistentNaming
		public SlateRef<Map> map;
		public SlateRef<Models.Contract> contract;

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
		public SlateRef<string> postponeSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> recruitSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> rejectSignal;

		[NoTranslate, TranslationHandle(Priority = 100)]
		public SlateRef<string> terminateSignal;

		public SlateRef<QuestPart.SignalListenMode?> signalListenMode;
		// ReSharper restore MemberCanBePrivate.Global
		// ReSharper restore InconsistentNaming

		protected override void RunInt()
		{
			try
			{
				Slate slate = QuestGen.slate;
				Quest quest = QuestGen.quest;
				quest.DescriptionPart("[questDescriptionBeforeAccepted]", quest.AddedSignal, quest.InitiateSignal,
					QuestPart.SignalListenMode.Always);

				quest.DescriptionPart("[questDescriptionAfterAccepted]", quest.InitiateSignal, null,
					QuestPart.SignalListenMode.OngoingOrNotYetAccepted);

				map.TryGetValue(slate, out Map colonyMap);
				contract.TryGetValue(slate, out Models.Contract cont);
				var payment = new QuestPart_ContractConclusion
				{
					inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
					outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate)),
					badSignal = QuestGenUtility.HardcodedSignalWithQuestID(badSignal.GetValue(slate)),
					joinSignal = QuestGenUtility.HardcodedSignalWithQuestID(joinSignal.GetValue(slate)),
					leaveSignal = QuestGenUtility.HardcodedSignalWithQuestID(leaveSignal.GetValue(slate)),
					postponeSignal = QuestGenUtility.HardcodedSignalWithQuestID(postponeSignal.GetValue(slate)),
					recruitSignal = QuestGenUtility.HardcodedSignalWithQuestID(recruitSignal.GetValue(slate)),
					rejectSignal = QuestGenUtility.HardcodedSignalWithQuestID(rejectSignal.GetValue(slate)),
					terminateSignal = QuestGenUtility.HardcodedSignalWithQuestID(terminateSignal.GetValue(slate)),
					initiateSignal = quest.InitiateSignal,
					signalListenMode =
						(signalListenMode.GetValue(slate) ?? QuestPart.SignalListenMode.OngoingOnly),
					contract = cont,
					map = colonyMap
				};

				QuestGen.quest.AddPart(payment);
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