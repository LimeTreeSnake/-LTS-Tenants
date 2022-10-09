
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Tenants.Language;

namespace Tenants.Settings {
    public class TenantsSettings : Mod {
        private readonly Settings settings;
        public TenantsSettings(ModContentPack content) : base(content) {
            settings = GetSettings<Settings>();
            settings.Initialize();
        }

        public override string SettingsCategory() {
            return "LTS Tenants";
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            settings.DoWindowContents(inRect);
        }
    }

    public class Settings : ModSettings {
        #region Fields
        private static IntRange days, courierDays;
        private static int rent, worldTenants;
        private static string rentBuffer, worldTenantsBuffer;
        private static int moodTicks;
        private static string moodTicksBuffer;
        private static int noticeCourierCost;
        private static string noticeCourierCostBuffer;
        private static bool killPenalty = true, paymentGold = false;

        private static bool firstPage = true;
        private static readonly float lineHeight = Text.LineHeight;
        private static readonly float margin = 4f;

        public static IEnumerable<ThingDef> Races = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race != null && x.RaceProps.Humanlike).Select(s => s.race);
        public static IEnumerable<PawnKindDef> CourierDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.HasModExtension<DefModExtensions.CoureierExtension>());
        public static PawnKindDef GetCourierByWeight => GenCollection.RandomElementByWeight(CourierDefs,
            delegate (PawnKindDef x) {
                float? val = x.GetModExtension<DefModExtensions.CoureierExtension>().ChoiceWeight;
                if (!val.HasValue) {
                    return 1f;
                }
                return val.GetValueOrDefault();
            });
        private static List<string> availableRaces = new List<string>() { "Human" };

        private Vector2 scrollPos;
        private static string Filter = "";

        #endregion Fields
        public static int MoodTicks => moodTicks;
        public static int NoticeCourierCost => noticeCourierCost;
        public static int Rent => rent;
        public static int WorldTenants => worldTenants;
        public static IntRange Days => days;
        public static IntRange CourierDays => courierDays;
        public static List<string> AvailableRaces => availableRaces;
        public static bool KillPenalty => killPenalty;
        public static bool PaymentGold => paymentGold;

        public override void ExposeData() {
            Scribe_Values.Look(ref rent, "Rent", 60, false);
            Scribe_Values.Look(ref worldTenants, "WorldTenants", 5, false);
            Scribe_Values.Look(ref days, "Days", new IntRange(3, 7));
            Scribe_Values.Look(ref courierDays, "CourierDays", new IntRange(5, 9));
            Scribe_Values.Look(ref moodTicks, "MoodTicks", 30000, false);
            Scribe_Values.Look(ref killPenalty, "KillPenalty", true);
            Scribe_Values.Look(ref paymentGold, "PaymentGold", false);
            Scribe_Values.Look(ref noticeCourierCost, "NoticeCourierCost", 100, false);
            Scribe_Collections.Look(ref availableRaces, "AvailableRaces", LookMode.Value);
            base.ExposeData();
        }

        public void Initialize() {
            if (rent == 0 || days == null || days.min == 0)
                Reset();
        }
        public static void Reset() {
            availableRaces = new List<string>() { "Human" };
            rentBuffer = "60";
            rent = 60;
            worldTenants = 5;
            worldTenantsBuffer = "5";
            noticeCourierCostBuffer = "100";
            noticeCourierCost = 100;
            moodTicksBuffer = "30000";
            moodTicks = 30000;
            killPenalty = true;
            paymentGold = false;
            rent = 50;
            days = new IntRange(3, 7);
            courierDays = new IntRange(5, 9);
        }
        public void DoWindowContents(Rect inRect) {
            try {
                Listing_Standard list = new Listing_Standard();
                list.Begin(inRect);
                if (list.ButtonText(Translate.DefaultSettings)) {
                    Reset();
                };
                list.Gap(2);
                if (list.ButtonText(Translate.ChangePage)) {
                    firstPage = !firstPage;
                }
                if (firstPage) {                    
                    //CourierEvent
                    Rect rectCourier = list.GetRect(lineHeight);
                    Widgets.Label(rectCourier, Translate.CourierDaysSpawn(courierDays.min, courierDays.max));
                    TooltipHandler.TipRegion(rectCourier, Translate.CourierDaysSpawnDesc);
                    Widgets.IntRange(list.GetRect(lineHeight), (int)list.CurHeight, ref courierDays, 5, 15);
                    list.Gap(2);
                    //Days
                    Widgets.Label(list.GetRect(lineHeight), Translate.TenancyDaysContract(days.min, days.max));
                    Widgets.IntRange(list.GetRect(lineHeight), (int)list.CurHeight, ref days, 3, 15);
                    list.GapLine(6);
                    //Rent
                    Widgets.Label(list.GetRect(lineHeight), Translate.TenancyRentContract(rent));
                    Widgets.IntEntry(list.GetRect(lineHeight), ref rent, ref rentBuffer);
                    list.Gap(2);
                    //Mood
                    Rect rectMood = list.GetRect(lineHeight);
                    Widgets.Label(rectMood, Translate.MoodTicks(moodTicks));
                    TooltipHandler.TipRegion(rectMood, Translate.MoodTicksDesc);
                    Widgets.IntEntry(list.GetRect(lineHeight), ref moodTicks, ref moodTicksBuffer);
                    list.Gap(2);
                    //Advertisement Cost
                    Widgets.Label(list.GetRect(lineHeight), Translate.AdvertisementCost(noticeCourierCost));
                    Widgets.IntEntry(list.GetRect(lineHeight), ref noticeCourierCost, ref noticeCourierCostBuffer);
                    list.Gap(2);
                    //Stored Tenants
                    Rect rectStoredTenants = list.GetRect(lineHeight);
                    Widgets.Label(rectStoredTenants, Translate.TenantsStored(worldTenants));
                    TooltipHandler.TipRegion(rectStoredTenants, Translate.TenantsStoredDesc);
                    Widgets.IntEntry(list.GetRect(lineHeight), ref worldTenants, ref worldTenantsBuffer);
                    list.GapLine(6);
                    //Accept Gold Penalty
                    Rect rectGoldPenalty = list.GetRect(lineHeight);
                    Widgets.CheckboxLabeled(rectGoldPenalty, Translate.GoldPayment, ref paymentGold);
                    TooltipHandler.TipRegion(rectGoldPenalty, Translate.GoldPaymentDesc);
                    list.GapLine(2);
                    //Kill Penalty
                    Rect rectKill = list.GetRect(lineHeight);
                    Widgets.CheckboxLabeled(rectKill, Translate.KillPenalty, ref killPenalty);
                    TooltipHandler.TipRegion(rectKill, Translate.KillPenaltyDesc);
                    list.GapLine(2);
                }
                else{
                    // Race Settings
                    list.GapLine(12f);
                    Rect rectRaces = list.GetRect(lineHeight);
                    Widgets.Label(rectRaces.LeftHalf(), Translate.Races);
                    Filter = Widgets.TextField(rectRaces.RightHalf(), Filter);
                    list.Gap(6f);

                    Rect optionsRect = list.GetRect(lineHeight * 13f);
                    Widgets.DrawMenuSection(optionsRect);

                    Rect tenantsRect = optionsRect.ContractedBy(margin * 2);
                    Rect TenantsViewRect = tenantsRect.ContractedBy(margin);
                    float num2 = (Races.Count() * lineHeight) / 3;
                    if (num2 < TenantsViewRect.height) {
                        num2 = TenantsViewRect.height;
                    }
                    TenantsViewRect.height = num2;
                    Widgets.BeginScrollView(tenantsRect, ref scrollPos, TenantsViewRect, true);
                    Listing_Standard tenantsList = new Listing_Standard(tenantsRect, () => scrollPos) {
                        ColumnWidth = ((TenantsViewRect.width / 3) - margin * 6)
                    };
                    tenantsList.Begin(TenantsViewRect);
                    foreach (ThingDef def in Races) {
                        if (def.defName.ToLower().Contains(Filter.ToLower())) {
                            bool contains = AvailableRaces.Contains(def.defName);
                            Rect raceRect = tenantsList.GetRect(lineHeight);
                            Widgets.CheckboxLabeled(raceRect, def.defName, ref contains, false);
                            if (contains == false && AvailableRaces.Contains(def.defName)) {
                                AvailableRaces.Remove(def.defName);
                            }
                            else if (contains == true && !AvailableRaces.Contains(def.defName)) {
                                AvailableRaces.Add(def.defName);
                            }
                        }
                    }
                    tenantsList.End();
                    Widgets.EndScrollView();
                }               
                list.End();
                Write();
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }

    }
}
