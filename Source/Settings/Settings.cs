using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Tenants.Language;

namespace Tenants.Settings
{
	public class TenantsSettings : Mod
	{
		private readonly Settings _settings;

		public TenantsSettings(ModContentPack content) : base(content)
		{
			_settings = this.GetSettings<Settings>();
			Settings.Initialize();
		}

		public override string SettingsCategory()
		{
			return "LTS Tenants";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			_settings.DoWindowContents(inRect);
		}
	}

	public class Settings : ModSettings
	{

		#region Fields

		private static IntRange _days, _courierDays;
		private static int _rent, _worldTenants;
		private static string _rentBuffer, _worldTenantsBuffer;
		private static int _moodTicks;
		private static string _moodTicksBuffer;
		private static int _noticeCourierCost;
		private static string _noticeCourierCostBuffer;
		private static bool _killPenalty = true, _paymentGold;
		private static bool _advertNoticeSound;

		private static bool _firstPage = true;
		private static readonly float _lineHeight = Text.LineHeight;
		private const float Margin = 4f;

		private static readonly IEnumerable<ThingDef> _races = DefDatabase<PawnKindDef>.AllDefsListForReading
			.Where(x => x.race != null && x.RaceProps.Humanlike)
			.Select(s => s.race)
			.Distinct();

		private static readonly IEnumerable<PawnKindDef> _courierDefs =
			DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x =>
				x.HasModExtension<DefModExtensions.CourierExtension>());

		public static PawnKindDef GetCourierByWeight => _courierDefs.RandomElementByWeight(delegate(PawnKindDef x)
		{
			float? val = x.GetModExtension<DefModExtensions.CourierExtension>().ChoiceWeight;
			return val.GetValueOrDefault();
		});

		private static List<string> _availableRaces = new List<string>()
		{
			"Human"
		};

		private Vector2 _scrollPos;
		private static string _filter = "";

		#endregion Fields

		public static int MoodTicks => _moodTicks;
		public static int NoticeCourierCost => _noticeCourierCost;
		public static int Rent => _rent;
		public static int WorldTenants => _worldTenants;
		public static IntRange Days => _days;
		public static IntRange CourierDays => _courierDays;
		public static List<string> AvailableRaces => _availableRaces;
		public static bool KillPenalty => _killPenalty;
		public static bool PaymentGold => _paymentGold;
		public static bool AdvertNoticeSound => _advertNoticeSound;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref _rent, "Rent", 60);
			Scribe_Values.Look(ref _worldTenants, "WorldTenants", 5);
			Scribe_Values.Look(ref _days, "Days", new IntRange(3, 7));
			Scribe_Values.Look(ref _courierDays, "CourierDays", new IntRange(5, 9));
			Scribe_Values.Look(ref _moodTicks, "MoodTicks", 30000);
			Scribe_Values.Look(ref _killPenalty, "KillPenalty", true);
			Scribe_Values.Look(ref _paymentGold, "PaymentGold");
			Scribe_Values.Look(ref _advertNoticeSound, "AdvertNoticeSound");
			Scribe_Values.Look(ref _noticeCourierCost, "NoticeCourierCost", 100);
			Scribe_Collections.Look(ref _availableRaces, "AvailableRaces", LookMode.Value);
			base.ExposeData();
		}

		public static void Initialize()
		{
			if (_rent == 0 || _days.min == 0)
			{
				Reset();
			}
		}

		public static void Reset()
		{
			_availableRaces = new List<string>()
			{
				"Human"
			};

			_rentBuffer = "60";
			_rent = 60;
			_worldTenants = 5;
			_worldTenantsBuffer = "5";
			_noticeCourierCostBuffer = "100";
			_noticeCourierCost = 100;
			_moodTicksBuffer = "30000";
			_moodTicks = 30000;
			_killPenalty = true;
			_paymentGold = false;
			_rent = 50;
			_days = new IntRange(3, 7);
			_courierDays = new IntRange(5, 9);
		}

		public void DoWindowContents(Rect inRect)
		{
			try
			{
				var list = new Listing_Standard();
				list.Begin(inRect);
				if (list.ButtonText(Translate.DefaultSettings))
				{
					Reset();
				}
				list.Gap(2);
				if (list.ButtonText(Translate.ChangePage))
				{
					_firstPage = !_firstPage;
				}

				if (_firstPage)
				{
					//CourierEvent
					Rect rectCourier = list.GetRect(_lineHeight);
					Widgets.Label(rectCourier, Translate.CourierDaysSpawn(_courierDays.min, _courierDays.max));
					TooltipHandler.TipRegion(rectCourier, Translate.CourierDaysSpawnDesc);
					Widgets.IntRange(list.GetRect(_lineHeight), (int)list.CurHeight, ref _courierDays, 5, 15);
					list.Gap(2);
					//Days
					Widgets.Label(list.GetRect(_lineHeight), Translate.TenancyDaysContract(_days.min, _days.max));
					Widgets.IntRange(list.GetRect(_lineHeight), (int)list.CurHeight, ref _days, 3, 15);
					list.GapLine(6);
					//Rent
					Widgets.Label(list.GetRect(_lineHeight), Translate.TenancyRentContract(_rent));
					Widgets.IntEntry(list.GetRect(_lineHeight), ref _rent, ref _rentBuffer);
					list.Gap(2);
					//Mood
					Rect rectMood = list.GetRect(_lineHeight);
					Widgets.Label(rectMood, Translate.MoodTicks(_moodTicks));
					TooltipHandler.TipRegion(rectMood, Translate.MoodTicksDesc);
					Widgets.IntEntry(list.GetRect(_lineHeight), ref _moodTicks, ref _moodTicksBuffer);
					list.Gap(2);
					//Advertisement Cost
					Widgets.Label(list.GetRect(_lineHeight), Translate.AdvertisementCost(_noticeCourierCost));
					Widgets.IntEntry(list.GetRect(_lineHeight), ref _noticeCourierCost, ref _noticeCourierCostBuffer);
					list.Gap(2);
					//Stored Tenants
					Rect rectStoredTenants = list.GetRect(_lineHeight);
					Widgets.Label(rectStoredTenants, Translate.TenantsStored(_worldTenants));
					TooltipHandler.TipRegion(rectStoredTenants, Translate.TenantsStoredDesc);
					Widgets.IntEntry(list.GetRect(_lineHeight), ref _worldTenants, ref _worldTenantsBuffer);
					list.GapLine(6);
					//Accept Gold Penalty
					Rect rectGoldPenalty = list.GetRect(_lineHeight);
					Widgets.CheckboxLabeled(rectGoldPenalty, Translate.GoldPayment, ref _paymentGold);
					TooltipHandler.TipRegion(rectGoldPenalty, Translate.GoldPaymentDesc);
					list.GapLine(2);
					//Kill Penalty
					Rect rectKill = list.GetRect(_lineHeight);
					Widgets.CheckboxLabeled(rectKill, Translate.KillPenalty, ref _killPenalty);
					TooltipHandler.TipRegion(rectKill, Translate.KillPenaltyDesc);
					list.GapLine(2);
					//Kill Penalty
					Rect rectAdvertSound = list.GetRect(_lineHeight);
					Widgets.CheckboxLabeled(rectAdvertSound, Translate.AdvertNoticeSound, ref _advertNoticeSound);
					TooltipHandler.TipRegion(rectAdvertSound, Translate.AdvertNoticeSoundDesc);
					list.GapLine(2);
				}
				else
				{
					// Race Settings
					list.GapLine();
					Rect rectRaces = list.GetRect(_lineHeight);
					Widgets.Label(rectRaces.LeftHalf(), Translate.Races);
					_filter = Widgets.TextField(rectRaces.RightHalf(), _filter);
					list.Gap(6f);

					Rect optionsRect = list.GetRect(_lineHeight * 13f);
					Widgets.DrawMenuSection(optionsRect);

					Rect tenantsRect = optionsRect.ContractedBy(Margin * 2);
					Rect tenantsViewRect = tenantsRect.ContractedBy(Margin);
					float num2 = (_races.Count() * _lineHeight) / 3;
					if (num2 < tenantsViewRect.height)
					{
						num2 = tenantsViewRect.height;
					}

					tenantsViewRect.height = num2;
					Widgets.BeginScrollView(tenantsRect, ref _scrollPos, tenantsViewRect);
					var tenantsList = new Listing_Standard(tenantsRect, () => _scrollPos)
					{
						ColumnWidth = ((tenantsViewRect.width / 3) - Margin * 6)
					};

					tenantsList.Begin(tenantsViewRect);
					foreach (ThingDef def in _races)
					{
						if (def.defName.ToLower().Contains(_filter.ToLower()))
						{
							bool contains = AvailableRaces.Contains(def.defName);
							Rect raceRect = tenantsList.GetRect(_lineHeight);
							Widgets.CheckboxLabeled(raceRect, def.LabelCap, ref contains);
							switch (contains)
							{
								case false when AvailableRaces.Contains(def.defName):
									AvailableRaces.Remove(def.defName);
									break;


								case true when !AvailableRaces.Contains(def.defName):
									AvailableRaces.Add(def.defName);
									break;
							}
						}
					}

					tenantsList.End();
					Widgets.EndScrollView();
				}

				list.End();
				this.Write();
			}
			catch (Exception ex)
			{
				Log.Message(ex.Message);
			}
		}

	}
}