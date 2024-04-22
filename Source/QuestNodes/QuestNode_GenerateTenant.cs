using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Tenants.Components;
using Tenants.Logic;
using Tenants.Models;
using Verse;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
namespace Tenants.QuestNodes
{
	public class QuestNode_GenerateTenant : QuestNode
	{
		[NoTranslate] public SlateRef<string> tenant;
		[NoTranslate] public SlateRef<string> gender;
		[NoTranslate] public SlateRef<string> genes;
		[NoTranslate] public SlateRef<string> addToList;
		[NoTranslate] public SlateRef<string> tenantFaction;
		[NoTranslate] public SlateRef<int> startDate;
		[NoTranslate] public SlateRef<int> endDate;
		[NoTranslate] public SlateRef<int> days;
		[NoTranslate] public SlateRef<int> rent;
		[NoTranslate] public SlateRef<int> rentSum;
		[NoTranslate] public SlateRef<bool> roomRequired;
		[NoTranslate] public SlateRef<bool> mayJoin;
		[NoTranslate] public SlateRef<bool> violenceEnabled;
		[NoTranslate] public SlateRef<int> ticks;
		[NoTranslate] public SlateRef<bool> rejected;
		[NoTranslate] public SlateRef<string> inSignalRemovePawn;
		[NoTranslate] public SlateRef<string> destroyedSignal;
		[NoTranslate] public SlateRef<string> killedSignal;
		[NoTranslate] public SlateRef<string> kidnappedSignal;
		[NoTranslate] public SlateRef<string> arrestedSignal;
		[NoTranslate] public SlateRef<string> surgeryViolationSignal;
		[NoTranslate] public SlateRef<string> banishedSignal;
		[NoTranslate] public SlateRef<string> psychicRitualTargetSignal;
		[NoTranslate] public SlateRef<string> contract;

		public SlateRef<Map> map;

		protected override void RunInt()
		{
			try
			{
				Quest quest = QuestGen.quest;
				Slate slate = QuestGen.slate;
				if (map.GetValue(slate) == null)
				{
					Log.Error("No map found for the quest slate");
					return;
				}

				map.TryGetValue(slate, out Map mapStuff);
				TenantsMapComponent comp = mapStuff.GetComponent<TenantsMapComponent>();
				if (comp == null)
				{
					comp = new TenantsMapComponent(mapStuff);
					mapStuff.components.Add(comp);
				}

				Faction faction = Find.FactionManager.FirstFactionOfDef(Defs.FactionDefOf.LTS_Tenant);
				if (faction == null)
				{
					faction = FactionGenerator.NewGeneratedFaction(
						new FactionGeneratorParms(Defs.FactionDefOf.LTS_Tenant));

					Find.FactionManager.Add(faction);
				}

				var request = new PawnGenerationRequest(
					TenancyLogic.GetRandomPawnKindDef(),
					faction,
					forbidAnyTitle: true,
					forcedXenotype: comp?.NoticeBoard()?._chosenXeno,
					forcedCustomXenotype: comp?.NoticeBoard()?._chosenCustomXeno,
					biologicalAgeRange: new FloatRange(15, comp?.NoticeBoard()?._maxAge ?? 50),
					fixedGender: comp?.NoticeBoard()?.GetForcedGender());
				
				Pawn tenantPawn = quest.GeneratePawn(request);
				if (tenantPawn == null)
				{
					Log.Error("Somehow failed to generate tenant in QuestNode_GenerateTenant");
					return;
				}

				slate.Set(tenant.GetValue(slate), tenantPawn);
				slate.Set(gender.GetValue(slate), tenantPawn.gender.GetLabel());
				slate.Set(genes.GetValue(slate),
					ModLister.BiotechInstalled ? tenantPawn.genes.XenotypeLabel : tenantPawn.def.label);

				slate.Set(tenantFaction.GetValue(slate), tenantPawn.Faction);
				QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), tenantPawn);

				quest.AddPart(new QuestPart_ExtraFaction
				{
					affectedPawns = new List<Pawn>()
					{
						tenantPawn
					},
					extraFaction = new ExtraFaction(tenantPawn.Faction, ExtraFactionType.HomeFaction),
					areHelpers = false,
					inSignalRemovePawn = inSignalRemovePawn.GetValue(slate)
				});

				Contract contracts = TenancyLogic.GenerateBasicTenancyContract(tenantPawn, comp);
				slate.Set(contract.GetValue(slate), contracts);
				slate.Set("days", contracts.LengthDays);
				slate.Set("rent", contracts._rent);
				slate.Set("rentSum", contracts._rent * contracts.LengthDays);
				slate.Set("ticks", contracts._length);
				slate.Set("startDate", contracts._startDate);
				slate.Set("endDate", contracts._endDate);
				slate.Set("roomRequired", contracts._singleRoomRequirement);
				slate.Set("mayJoin", contracts._mayJoin);
				slate.Set("violenceEnabled", contracts._violenceEnabled);

				var signalWorker = new QuestPart_GenerateTenant
				{
					inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
					inSignalDestroyed = QuestGenUtility.HardcodedSignalWithQuestID(destroyedSignal.GetValue(slate)),
					inSignalKilled = QuestGenUtility.HardcodedSignalWithQuestID(killedSignal.GetValue(slate)),
					inSignalBanished = QuestGenUtility.HardcodedSignalWithQuestID(banishedSignal.GetValue(slate)),
					inSignalKidnapped = QuestGenUtility.HardcodedSignalWithQuestID(kidnappedSignal.GetValue(slate)),
					inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID(surgeryViolationSignal.GetValue(slate)),
					inSignalArrested = QuestGenUtility.HardcodedSignalWithQuestID(arrestedSignal.GetValue(slate)),
					inSignalPsychicRitualTarget =
						QuestGenUtility.HardcodedSignalWithQuestID(psychicRitualTargetSignal.GetValue(slate)),
					tenant = tenantPawn,
					signalListenMode = QuestPart.SignalListenMode.Always
				};

				quest.AddPart(signalWorker);


				if (!Settings.Settings.KillPenalty)
				{
					slate.Set("rejected", false);
					comp.TenantKills = 0;
					return;
				}
				
				if (Settings.Settings.DebugLog)
				{
					Log.Message(comp.TenantKills.ToString());
				}

				slate.Set("rejected", comp.TenantKills > 0);

			}
			catch (Exception ex)
			{
				Log.Message("Error at QuestNode_GenerateTenant RunInt: " + ex.Message);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			return slate.Exists("map");
		}
	}
}