
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Tenants.Settings {
    public class TenantsSettings : Mod {
        private readonly Settings settings;

        public TenantsSettings(ModContentPack content) : base(content) {
            settings = GetSettings<Settings>();
            settings.Initialize();
        }

        public override string SettingsCategory() {
            return "Tenants";
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            settings.DoWindowContents(inRect);
        }
    }

    public class Settings : ModSettings {
        #region Fields
        private static int minDays;
        private static int maxDays;
        private static int minRent;
        private static int maxRent;
        private static int ticksUntil;
        private static string ticksUntilBuffer;
        private static int noticeCourierCost;
        private static string noticeCourierCostBuffer;
        private static IntRange rent, days;
        private static bool killPenalty = true;

        public static IEnumerable<ThingDef> Races = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race != null && x.RaceProps.Humanlike).Select(s => s.race).Distinct();
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
        public static int MinDays => minDays;
        public static int MaxDays => maxDays;
        public static int MinRent => minRent;
        public static int MaxRent => maxRent;
        public static int TicksUntil => ticksUntil;
        public static int NoticeCourierCost => noticeCourierCost;
        public static IntRange Rent => rent;
        public static IntRange Days => days;
        public static List<string> AvailableRaces => availableRaces;
        public static bool KillPenalty => killPenalty;

        public override void ExposeData() {
            Scribe_Values.Look(ref minDays, "MinDays", 3, false);
            Scribe_Values.Look(ref maxDays, "MaxDays", 7, false);
            Scribe_Values.Look(ref minRent, "MinRent", 50, false);
            Scribe_Values.Look(ref maxRent, "MaxRent", 250, false);
            Scribe_Values.Look(ref ticksUntil, "TicksUntil", 60000, false);
            Scribe_Values.Look(ref killPenalty, "KillPenalty", true);
            Scribe_Values.Look(ref noticeCourierCost, "NoticeCourierCost", 100, false);
            Scribe_Values.Look(ref days, "Days", new IntRange(3, 7));
            Scribe_Values.Look(ref rent, "Rent", new IntRange(50, 100));
            Scribe_Collections.Look(ref availableRaces, "AvailableRaces", LookMode.Value);
            base.ExposeData();
        }

        public void Initialize() {
            if (rent == null || days == null || rent.min == 0 || days.min == 0)
                Reset();
        }
        public static void Reset() {
            availableRaces = new List<string>() { "Human" };
            minDays = 1;
            maxDays = 30;
            minRent = 50;
            maxRent = 250;
            noticeCourierCost = 100;
            ticksUntil = 60000;
            killPenalty = true;
            rent = new IntRange(50, 100);
            days = new IntRange(3, 7);
        }
        public void DoWindowContents(Rect inRect) {
            try {
                inRect.yMin += 20f;
                inRect.yMax -= 20f;
                float lineHeight = Text.LineHeight;
                Rect PageRect = new Rect(inRect.x, inRect.y, inRect.width - 30f, inRect.height - 30f);
                Listing_Standard list = new Listing_Standard();
                list.Begin(PageRect);
                if (list.ButtonText(Language.Translate.DefaultSettings)) {
                    Reset();
                };
                Rect rect1 = list.GetRect(lineHeight);
                Rect rect2 = list.GetRect(lineHeight);
                Rect rect3 = list.GetRect(lineHeight);
                Rect rect4 = list.GetRect(lineHeight);
                Widgets.Label(rect1.LeftHalf(), Language.Translate.TenancyDaysContract(days.min, days.max));
                Widgets.IntRange(rect2.LeftHalf(), (int)list.CurHeight, ref days, minDays, maxDays);
                Widgets.Label(rect3.LeftHalf(), Language.Translate.TenancyRentContract(rent.min, rent.max));
                Widgets.IntRange(rect4.LeftHalf(), (int)list.CurHeight, ref rent, minRent, maxRent);

                Widgets.Label(rect1.RightHalf(), Language.Translate.AdvertisementCost(noticeCourierCost));
                Widgets.IntEntry(rect2.RightHalf(), ref noticeCourierCost, ref noticeCourierCostBuffer);
                Widgets.Label(rect3.RightHalf(), Language.Translate.TicksUntil(ticksUntil));
                Widgets.IntEntry(rect4.RightHalf(), ref ticksUntil, ref ticksUntilBuffer);

                Rect rect5 = list.GetRect(lineHeight);
                Widgets.CheckboxLabeled(rect5.LeftHalf(), Language.Translate.KillPenalty, ref killPenalty);


                list.GapLine(12f);
                Rect rect6 = list.GetRect(lineHeight);
                Widgets.Label(rect6.LeftHalf(), Language.Translate.Races);
                Filter = Widgets.TextField(rect6.RightHalf(), Filter);
                Rect optionsRect = list.GetRect(lineHeight * 10f);
                float num2 = (Races.Count() * lineHeight) / 3;
                if (num2 < optionsRect.height) {
                    num2 = optionsRect.height;
                }
                float margin = 4f;
                Widgets.DrawMenuSection(optionsRect);
                Rect tenantsRect = optionsRect.ContractedBy(margin);

                Rect TenantsViewRect = new Rect(0f, 0f, tenantsRect.width - 16f, num2);
                Widgets.BeginScrollView(tenantsRect, ref this.scrollPos, TenantsViewRect, true);
                Listing_Standard tenantsList = new Listing_Standard(tenantsRect, () => this.scrollPos) {
                    ColumnWidth = ((tenantsRect.width - 16) / 3) - (margin * 3)
                };
                tenantsList.Begin(TenantsViewRect);
                tenantsList.ColumnWidth = ((rect2.width - 50f) / 3f);
                foreach (ThingDef def in Races) {
                    if (def.defName.ToLower().Contains(Filter.ToLower())) {
                        bool contains = AvailableRaces.Contains(def.defName);
                        Rect weaponRect = tenantsList.GetRect(lineHeight);
                        Widgets.CheckboxLabeled(weaponRect, def.defName, ref contains, false);
                        if (contains == false && AvailableRaces.Contains(def.defName)) {
                            AvailableRaces.Remove(def.defName);
                        }
                        else if (contains == true && !AvailableRaces.Contains(def.defName)) {
                            AvailableRaces.Add(def.defName);
                        }
                    }
                }
                tenantsList.End();
                list.End();
                Widgets.EndScrollView();
                Write();
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }

    }
}
