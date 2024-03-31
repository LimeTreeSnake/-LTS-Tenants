using System;
using System.Collections.Generic;
using System.Linq;
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
				contract._rent = (int?)component.NoticeBoard()?.CalculateRent() ?? Settings.Settings.Rent;

				return contract;
			}
			catch (Exception e)
			{
				Log.Message("Error generating GenerateBasicTenancyContract: " + e.Message);
				return null;
			}
		}
	}
}