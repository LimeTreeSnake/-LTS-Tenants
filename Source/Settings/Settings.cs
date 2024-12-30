using System;
using UnityEngine;
using Verse;
using Tenants.Language;
using LTS_Systems.GUI;

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
        private static bool _killPenalty = true, _sameIdeo = true, _advertNoticeSound = true;
        private static bool _paymentGold, _allowArchiteTenants, _allowNonFleshTenants;
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
        public static bool AdvertNoticeSound => _advertNoticeSound;
        public static bool PaymentGold => _paymentGold;
        public static bool AllowArchiteTenants => _allowArchiteTenants;
        public static bool AllowNonFleshTenants => _allowNonFleshTenants;
        public static bool SameIdeo => _sameIdeo;
        public static bool DebugLog => _debugLog;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _rent, "Rent", 50);
            Scribe_Values.Look(ref _days, "Days", new IntRange(3, 7));
            Scribe_Values.Look(ref _courierDays, "CourierDays", new IntRange(5, 9));
            Scribe_Values.Look(ref _moodTicks, "MoodTicks", 30000);
            Scribe_Values.Look(ref _killPenalty, "KillPenalty", true);
            Scribe_Values.Look(ref _sameIdeo, "SameIdeo", true);
            Scribe_Values.Look(ref _advertNoticeSound, "AdvertNoticeSound");
            Scribe_Values.Look(ref _paymentGold, "PaymentGold");
            Scribe_Values.Look(ref _allowArchiteTenants, "AllowArchiteTenants");
            Scribe_Values.Look(ref _allowNonFleshTenants, "AllowNonFleshTenants");
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
                var topSettingsRect = list.GetRect(_lineHeight * 2);
                topSettingsRect.SplitVertically(inRect.width / 1.5f,
                    out var topSettingsRectLeft,
                    out var topSettingsRectRight);

                topSettingsRectLeft.SplitVertically(topSettingsRectLeft.width / 3,
                    out var _,
                    out var otherSettingsRectRight);

                otherSettingsRectRight.SplitVertically(otherSettingsRectRight.width / 2,
                    out var _,
                    out var _);

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
                    var headerRect = list.GetRect(_lineHeight * 1.5f);
                    Widgets.Label(headerRect, LTS_Systems.Language.Translate.GeneralSettings);
                    Text.Font = GameFont.Small;

                    //ToggleSettings
                    //Accept Gold
                    list.DrawCheckbox(_lineHeight, ref _paymentGold,
                        Translate.GoldPayment, Translate.GoldPaymentDesc, true);

                    //Kill Penalty
                    list.DrawCheckbox(_lineHeight, ref _killPenalty,
                        Translate.KillPenalty, Translate.KillPenaltyDesc, true);
                    //Same Ideo
                    list.DrawCheckbox(_lineHeight, ref _sameIdeo,
                        Translate.SameIdeo, Translate.SameIdeoDesc, true);

                    //Advertisement Noise
                    list.DrawCheckbox(_lineHeight, ref _advertNoticeSound,
                        Translate.AdvertNoticeSound, Translate.AdvertNoticeSoundDesc, true);

                    if (ModLister.BiotechInstalled)
                    {
                        //Archite
                        list.DrawCheckbox(_lineHeight, ref _allowArchiteTenants,
                            Translate.AllowArchite, Translate.AllowArchiteDesc, true);

                        //NonFlesh
                        list.DrawCheckbox(_lineHeight, ref _allowNonFleshTenants,
                            Translate.NonFlesh, Translate.NonFleshDesc, true);
                    }

                    list.Gap();

                    //Input Settings
                    //Rent
                    var rentRect = list.GetRect(_lineHeight).LeftHalf();
                    if (rentRect.TextFieldNumeric(ref _rent, 1, 999,
                            Translate.TenancyRentContract, Translate.TenancyRentContractDesc))
                    {
                        _changesMade = true;
                    }

                    list.Gap(4);

                    //Mood
                    var moodRect = list.GetRect(_lineHeight).LeftHalf();
                    if (moodRect.TextFieldNumeric(ref _moodTicks, 1, 90000,
                            Translate.MoodTicks, Translate.MoodTicksDesc))
                    {
                        _changesMade = true;
                    }

                    list.Gap(4);

                    //Advertisement Cost
                    var advertRect = list.GetRect(_lineHeight).LeftHalf();
                    if (advertRect.TextFieldNumeric(ref _noticeCourierCost, 1, 999,
                            Translate.AdvertisementCost, Translate.AdvertisementCostDesc))
                    {
                        _changesMade = true;
                    }

                    list.Gap(4);

                    //CourierEvent
                    list.DrawIntRangeSlider(_lineHeight, ref _courierDays, 5, 15,
                        Translate.CourierDaysSpawn(_courierDays.min, _courierDays.max), Translate.CourierDaysSpawnDesc);

                    list.Gap(4);

                    //Days
                    list.DrawIntRangeSlider(_lineHeight, ref _days, 3, 15,
                        Translate.TenancyDaysContract(_days.min, _days.max));

                    list.GapLine(6);
                    list.Gap();

                    list.DrawCheckbox(_lineHeight, ref _debugLog,
                        LTS_Systems.Language.Translate.DebuggingMessages, null, false, false);
                }

                if (_changesMade)
                {
                    //Not necessary atm
                    _changesMade = false;
                }

                list.End();
                this.Write();

            }
            catch (Exception ex)
            {
                Log.Error($"LTS_Tenants Error - Settings DoWindowContents: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}