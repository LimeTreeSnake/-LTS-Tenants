
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
        //IDEA: Add to logic, min/max days until courier.
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
        private static int minDays, maxDays;
        private static IntRange days;
        private static int rent;
        private static string rentBuffer;
        private static int moodTicks;
        private static string moodTicksBuffer;
        private static int noticeCourierCost;
        private static string noticeCourierCostBuffer;
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
        public static int MoodTicks => moodTicks;
        public static int NoticeCourierCost => noticeCourierCost;
        public static int Rent => rent;
        public static IntRange Days => days;
        public static List<string> AvailableRaces => availableRaces;
        public static bool KillPenalty => killPenalty;

        public override void ExposeData() {
            Scribe_Values.Look(ref rent, "Rent", 60, false);
            Scribe_Values.Look(ref minDays, "MinDays", 3, false);
            Scribe_Values.Look(ref maxDays, "MaxDays", 7, false);
            Scribe_Values.Look(ref days, "Days", new IntRange(3, 7));
            Scribe_Values.Look(ref moodTicks, "MoodTicks", 30000, false);
            Scribe_Values.Look(ref killPenalty, "KillPenalty", true);
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
            minDays = 1;
            maxDays = 30;
            rentBuffer = "60";
            rent = 60;
            noticeCourierCostBuffer = "100";
            noticeCourierCost = 100;
            moodTicksBuffer = "30000";
            moodTicks = 30000;
            killPenalty = true;
            rent = 50;
            days = new IntRange(3, 7);
        }
        public void DoWindowContents(Rect inRect) {
            try {
                float margin = 4f;
                inRect.yMin += 20f;
                inRect.yMax -= 20f;
                float lineHeight = Text.LineHeight;
                Rect PageRect = new Rect(inRect.x, inRect.y, inRect.width - 30f, inRect.height - 30f);
                Listing_Standard list = new Listing_Standard();
                list.Begin(PageRect);
                if (list.ButtonText(Language.Translate.DefaultSettings)) {
                    Reset();
                };
                //Top Settings
                Rect rect1 = list.GetRect(lineHeight);
                Rect rect2 = list.GetRect(lineHeight);
                list.Gap(margin);
                Rect rect3 = list.GetRect(lineHeight);
                Rect rect4 = list.GetRect(lineHeight);
                list.Gap(margin);
                Rect rect5 = list.GetRect(lineHeight);
                Widgets.Label(rect1.LeftHalf(), Language.Translate.TenancyDaysContract(days.min, days.max));
                Widgets.IntRange(rect2.LeftHalf(), (int)list.CurHeight, ref days, minDays, maxDays);
                Widgets.Label(rect3.LeftHalf(), Language.Translate.TenancyRentContract(rent));
                Widgets.IntEntry(rect4.LeftHalf(), ref rent, ref rentBuffer);

                Widgets.Label(rect1.RightHalf(), Language.Translate.AdvertisementCost(noticeCourierCost));
                Widgets.IntEntry(rect2.RightHalf(), ref noticeCourierCost, ref noticeCourierCostBuffer);
                Widgets.Label(rect3.RightHalf(), Language.Translate.MoodTicks(moodTicks));
                TooltipHandler.TipRegion(rect3.RightHalf(), Language.Translate.MoodTicksDesc);
                Widgets.IntEntry(rect4.RightHalf(), ref moodTicks, ref moodTicksBuffer);

                Widgets.CheckboxLabeled(rect5.LeftHalf(), Language.Translate.KillPenalty, ref killPenalty);
                TooltipHandler.TipRegion(rect5.LeftHalf(), Language.Translate.KillPenaltyDesc);

                // Race Settings
                list.GapLine(12f);
                Rect rect6 = list.GetRect(lineHeight);
                Widgets.Label(rect6.LeftHalf(), Language.Translate.Races);
                Filter = Widgets.TextField(rect6.RightHalf(), Filter);
                list.Gap(6f);

                Rect optionsRect = list.GetRect(lineHeight * 10f);
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
                    ColumnWidth = ((TenantsViewRect.width / 3) - margin * 5)
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
