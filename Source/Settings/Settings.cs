using System;
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
		private static int _rent;
		private static int _moodTicks;
		private static int _noticeCourierCost;
		private static bool _killPenalty = true, _paymentGold;
		private static bool _advertNoticeSound;
		private static bool _debugLog;

		// Settings Stuff
		private static readonly float _lineHeight = Text.LineHeight;
		private static bool _changesMade;
		//private static bool _page1 = true, _page2;

		#endregion Fields

		public static int MoodTicks => _moodTicks;
		public static int NoticeCourierCost => _noticeCourierCost;
		public static int Rent => _rent;
		public static IntRange Days => _days;
		public static IntRange CourierDays => _courierDays;
		public static bool KillPenalty => _killPenalty;
		public static bool PaymentGold => _paymentGold;
		public static bool AdvertNoticeSound => _advertNoticeSound;
		public static bool DebugLog => _debugLog;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref _rent, "Rent", 50);
			Scribe_Values.Look(ref _days, "Days", new IntRange(3, 7));
			Scribe_Values.Look(ref _courierDays, "CourierDays", new IntRange(5, 9));
			Scribe_Values.Look(ref _moodTicks, "MoodTicks", 30000);
			Scribe_Values.Look(ref _killPenalty, "KillPenalty", true);
			Scribe_Values.Look(ref _paymentGold, "PaymentGold");
			Scribe_Values.Look(ref _advertNoticeSound, "AdvertNoticeSound");
			Scribe_Values.Look(ref _debugLog, "DebugLog");
			Scribe_Values.Look(ref _noticeCourierCost, "NoticeCourierCost", 100);
			base.ExposeData();
		}

		public static void Initialize()
		{
			if (_rent == 0 || _days.min == 0)
			{
				Reset();
			}
		}

		private static void Reset()
		{
			_rent = 50;
			_noticeCourierCost = 100;
			_moodTicks = 30000;
			_killPenalty = true;
			_paymentGold = false;
			_days = new IntRange(3, 7);
			_courierDays = new IntRange(5, 9);
			_advertNoticeSound = false;
			_debugLog = false;
		}

		public void DoWindowContents(Rect inRect)
		{
			try
			{
				var list = new Listing_Standard();
				list.Begin(inRect);

				//Top Settings
				Rect topSettingsRect = list.GetRect(_lineHeight * 2);
				topSettingsRect.SplitVertically(inRect.width / 1.5f,
					out Rect topSettingsRectLeft,
					out Rect topSettingsRectRight);

				topSettingsRectLeft.SplitVertically(topSettingsRectLeft.width / 3,
					out Rect _,
					out Rect otherSettingsRectRight);

				otherSettingsRectRight.SplitVertically(otherSettingsRectRight.width / 2,
					out Rect _,
					out Rect _);

				// if (Widgets.ButtonText(generalSettingsRect.ContractedBy(0, Margin),
				// 	    LTS_Systems.Language.Translate.GeneralSettings))
				// {
				// 	_page1 = true;
				// 	_page2 = false;
				// }
				//
				// if (Widgets.ButtonText(rectQualitySettings.ContractedBy(0, Margin),
				// 	    Translate.SettingsCourier))
				// {
				// 	_page1 = false;
				// 	_page2 = true;
				// }

				if (Widgets.ButtonText(topSettingsRectRight.RightHalf().BottomHalf(),
					    LTS_Systems.Language.Translate.DefaultSettings))
				{
					Reset();
					_changesMade = true;
				}

				list.GapLine(2);
				if (true)
				{
					//Header
					Text.Font = GameFont.Medium;
					Rect headerRect = list.GetRect(_lineHeight * 1.5f);
					Widgets.Label(headerRect, LTS_Systems.Language.Translate.GeneralSettings);
					Text.Font = GameFont.Small;

					//ToggleSettings
					//Accept Gold Penalty
					Rect rectGoldPenalty = list.GetRect(_lineHeight).LeftHalf().LeftHalf();
					Widgets.CheckboxLabeled(rectGoldPenalty, Translate.GoldPayment(), ref _paymentGold);
					if (Mouse.IsOver(rectGoldPenalty))
					{
						Widgets.DrawHighlight(rectGoldPenalty);
					}

					TooltipHandler.TipRegion(rectGoldPenalty, Translate.GoldPaymentDesc());
					//Kill Penalty
					Rect rectKill = list.GetRect(_lineHeight).LeftHalf().LeftHalf();
					Widgets.CheckboxLabeled(rectKill, Translate.KillPenalty(), ref _killPenalty);
					if (Mouse.IsOver(rectKill))
					{
						Widgets.DrawHighlight(rectKill);
					}

					TooltipHandler.TipRegion(rectKill, Translate.KillPenaltyDesc());
					//Kill Penalty
					Rect rectAdvertSound = list.GetRect(_lineHeight).LeftHalf().LeftHalf();
					Widgets.CheckboxLabeled(rectAdvertSound, Translate.AdvertNoticeSound(), ref _advertNoticeSound);
					if (Mouse.IsOver(rectAdvertSound))
					{
						Widgets.DrawHighlight(rectAdvertSound);
					}

					TooltipHandler.TipRegion(rectAdvertSound, Translate.AdvertNoticeSoundDesc());
					list.Gap(2);

					//Input Settings
					//Rent
					Rect rentRect = list.GetRect(_lineHeight).LeftHalf();
					rentRect.SplitVertically(rentRect.width * 0.4f,
						out Rect leftPartRentLabel,
						out Rect leftPartRentField);

					Widgets.Label(leftPartRentLabel, Translate.TenancyRentContract());
					TooltipHandler.TipRegion(rentRect, Translate.TenancyRentContractDesc());
					int rent = _rent;
					string rentBuffer = rent.ToString();
					Widgets.TextFieldNumeric(leftPartRentField, ref rent, ref rentBuffer, 1, 999);
					if (rent != _rent)
					{
						_changesMade = true;
						_rent = rent;
					}

					list.Gap(2);
					//Mood
					Rect moodRect = list.GetRect(_lineHeight).LeftHalf();
					moodRect.SplitVertically(moodRect.width * 0.4f,
						out Rect leftPartMoodLabel,
						out Rect leftPartMoodField);

					Widgets.Label(leftPartMoodLabel, Translate.MoodTicks());
					TooltipHandler.TipRegion(moodRect, Translate.MoodTicksDesc());
					int mood = _moodTicks;
					string moodBuffer = mood.ToString();
					Widgets.TextFieldNumeric(leftPartMoodField, ref mood, ref moodBuffer, 1, 90000);
					if (mood != _moodTicks)
					{
						_changesMade = true;
						_moodTicks = mood;
					}

					list.Gap(2);
					//Advertisement Cost
					Rect advertRect = list.GetRect(_lineHeight).LeftHalf();
					advertRect.SplitVertically(advertRect.width * 0.4f,
						out Rect leftPartAdvertLabel,
						out Rect leftPartAdvertField);

					Widgets.Label(leftPartAdvertLabel, Translate.AdvertisementCost());
					TooltipHandler.TipRegion(advertRect, Translate.AdvertisementCostDesc());
					int advert = _noticeCourierCost;
					string advertBuffer = advert.ToString();
					Widgets.TextFieldNumeric(leftPartAdvertField, ref advert, ref advertBuffer, 1, 999);
					if (advert != _noticeCourierCost)
					{
						_changesMade = true;
						_noticeCourierCost = advert;
					}

					list.Gap(2);

					//CourierEvent
					Rect rectCourier = list.GetRect(_lineHeight);
					Widgets.Label(rectCourier, Translate.CourierDaysSpawn(_courierDays.min, _courierDays.max));
					TooltipHandler.TipRegion(rectCourier, Translate.CourierDaysSpawnDesc());
					Widgets.IntRange(list.GetRect(_lineHeight), (int)list.CurHeight, ref _courierDays, 5, 15);
					list.Gap(2);

					//Days
					Widgets.Label(list.GetRect(_lineHeight), Translate.TenancyDaysContract(_days.min, _days.max));
					Widgets.IntRange(list.GetRect(_lineHeight), (int)list.CurHeight, ref _days, 3, 15);
					list.GapLine(6);
					
					//Kill Penalty
					Rect rectDebug = list.GetRect(_lineHeight).LeftHalf().LeftHalf();
					Widgets.CheckboxLabeled(rectDebug, "Dev Logs", ref _debugLog);
					if (Mouse.IsOver(rectDebug))
					{
						Widgets.DrawHighlight(rectDebug);
					}
				}

				if (_changesMade)
				{
					_changesMade = false;
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