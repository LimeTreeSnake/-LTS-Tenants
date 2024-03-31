using RimWorld;
using Tenants.Components;
using Tenants.Models;
using Verse;

namespace Tenants.QuestNodes
{
	public class QuestPart_GenerateTenant: QuestPartActivable
	{
		//Vanilla signals
		public string inSignalDestroyed;
		public string inSignalKilled;
		public string inSignalArrested;
		public string inSignalSurgeryViolation;
		public string inSignalKidnapped;
		public string inSignalBanished;
		public string inSignalPsychicRitualTarget;
		public Pawn tenant;
		
		protected override void ProcessQuestSignal(Signal signal)
		{
			base.ProcessQuestSignal(signal);
			
			if (Settings.Settings.DebugLog)
			{
				Log.Message("ProcessQuestSignal: " + signal);
			}
			
			TenantsMapComponent comp = tenant?.Map?.GetComponent<TenantsMapComponent>();

			if (comp == null)
			{
				return;
			}
			
			Pawn pawn;
			if (signal.tag == inSignalDestroyed &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("DestroyedSignal");
			}
			if (signal.tag == inSignalKilled &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("KilledSignal");
			}
			if (signal.tag == inSignalArrested &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("inSignalArrested");
			}
			if (signal.tag == inSignalSurgeryViolation &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("inSignalSurgeryViolation");
			}
			if (signal.tag == inSignalKidnapped &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("inSignalKidnapped");
			}
			if (signal.tag == inSignalBanished &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				Log.Message("inSignalBanished");
			}
			if (signal.tag == inSignalPsychicRitualTarget &&
			    signal.args.TryGetArg("SUBJECT", out pawn) &&
			    tenant == pawn)
			{
				comp.TenantKills++;
				Log.Message("inSignalPsychicRitualTarget");
			}
		}
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref tenant, "Tenant");
			Scribe_Values.Look(ref inSignalDestroyed, "InSignalDestroyed");
			Scribe_Values.Look(ref inSignalArrested, "InSignalArrested");
			Scribe_Values.Look(ref inSignalSurgeryViolation, "InSignalSurgeryViolation");
			Scribe_Values.Look(ref inSignalPsychicRitualTarget, "InSignalPsychicRitualTarget");
			Scribe_Values.Look(ref inSignalKidnapped, "InSignalKidnapped");
			Scribe_Values.Look(ref inSignalBanished, "InSignalBanished");
		}
	}
}