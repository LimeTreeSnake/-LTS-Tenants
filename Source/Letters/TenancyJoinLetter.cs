using System.Collections.Generic;
using Tenants.Language;
using RimWorld;
using Tenants.Models;
using Verse;

namespace Tenants.Letters
{
	public class TenancyJoinLetter : ChoiceLetter
	{
		
		public string signalAccept;
		public string signalReject;
		
		public override bool CanDismissWithRightClick => false;

		public override bool CanShowInLetterStack
		{
			get
			{
				if (base.CanShowInLetterStack && this.quest != null)
				{
					if (this.quest.State != QuestState.Ongoing)
					{
						return this.quest.State == QuestState.NotYetAccepted;
					}
					return true;
				}
				return false;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (this.ArchivedOnly)
				{
					yield return this.Option_Close;
					yield break;
				}

				DiaOption optionAgree = new DiaOption(Translate.ContractAgree);
				DiaOption optionReject = new DiaOption(Translate.ContractReject);
				DiaOption optionPostpone = new DiaOption(Translate.ContractPostpone);
				optionAgree.action = delegate
				{
					Find.SignalManager.SendSignal(new Signal(signalAccept));
					Find.LetterStack.RemoveLetter(this);
				};
				optionAgree.resolveTree = true;
				optionReject.action = delegate
				{
					Find.SignalManager.SendSignal(new Signal(signalReject));
					Find.LetterStack.RemoveLetter(this);
				};
				optionReject.resolveTree = true;
				optionPostpone.action = delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				optionPostpone.resolveTree = true;
				yield return optionAgree;
				yield return optionReject;
				yield return optionPostpone;
				if (this.lookTargets.IsValid())
				{
					yield return this.Option_JumpToLocationAndPostpone;
				}
				yield return this.Option_Postpone;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref signalAccept, "signalAccept");
			Scribe_Values.Look(ref signalReject, "signalReject");
		}
	}
}