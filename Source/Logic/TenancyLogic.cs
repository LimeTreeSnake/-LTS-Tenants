using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.QuestGen;
using Tenants.Components;
using Tenants.Things;
using Verse;

namespace Tenants.Logic
{
	public static class TenancyLogic
	{
		private static List<PawnKindDef> _availablePawnKinds;

		private static void Initiate()
		{
			_availablePawnKinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x =>
					x.HasModExtension<DefModExtensions.TenancyExtension>())
				.ToList();
		}

		public static PawnKindDef GetRandomPawnKindDef()
		{
			if (_availablePawnKinds.NullOrEmpty())
			{
				Initiate();
			}

			return _availablePawnKinds.RandomElementByWeight(x =>
				x.GetModExtension<DefModExtensions.TenancyExtension>().choiceWeight);
		}

		public static float AgeValueCalculator(int age)
		{
			if (age > 50)
			{
				return 1;
			}

			int below = 50 - age;
			int decrementCount = below / 10;

			return 1 + decrementCount * 0.5f;
		}

		public static void AppendListData<T>(StringBuilder stringBuilder, string header, IEnumerable<T> items,
			Func<T, string> itemLabelFunc)
		{
			var itemList = items.ToList();

			stringBuilder.AppendInNewLine(header.Colorize(ColoredText.NameColor));
			stringBuilder.AppendLine();
			stringBuilder.Append("   ");

			if (itemList.Any())
			{
				string itemText = string.Join(" - ", itemList.Select(item => itemLabelFunc(item)));
				stringBuilder.Append(itemText);
			}
			else
			{
				stringBuilder.Append(" - ");
			}
		}
		
		public static void AppendWorkTypes(StringBuilder stringBuilder, string header, WorkTags workTags)
		{
			var relevantWorkTypes = DefDatabase<WorkTypeDef>.AllDefs
				.Where(def => (def.workTags & workTags) > WorkTags.None)
				.Select(def => def.pawnLabel);

			AppendListData(stringBuilder, header, relevantWorkTypes, label => label);
		}

		public static Models.Contract GenerateBasicTenancyContract(Pawn tenant, TenantsMapComponent component)
		{
			try
			{
				var contract = new Models.Contract
				{
					_tenant = tenant,
					_length = Rand.Range(Settings.Settings.Days.min, Settings.Settings.Days.max) * 60000,
					_startDate = Find.TickManager.TicksGame
				};

				contract._endDate = Find.TickManager.TicksAbs + contract._length + 60000;
				contract._singleRoomRequirement = component.NoticeBoard()?._singleRoom ?? false;
				contract._violenceEnabled = component.NoticeBoard()?._violenceEnabled ?? true;
				contract._mayJoin = component.NoticeBoard()?._mayJoin ?? false;
				contract._rent = (int?)component.NoticeBoard()?.CalculateRent() ?? Settings.Settings.Rent;

				return contract;
			}
			catch (Exception ex)
			{
				Log.Error($"LTS_Tenants Error - GenerateBasicTenancyContract: {ex.Message}\n{ex.StackTrace}");
				return null;
			}
		}
	}
}