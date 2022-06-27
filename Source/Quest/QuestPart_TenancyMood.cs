using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Tenants.QuestNodes {
    public class QuestPart_TenancyMood : QuestPartActivable {
        public Models.Contract contract;
        public float thresholdLow;
        public float thresholdHigh;

        public int minTicksBelowThreshold;
        public int minTicksAboveThreshold;

        public bool showAlert = true;

        private int moodBelowThresholdTicks;
        private int moodAboveThresholdTicks;
        public string OutSignalFailed => "Quest" + quest.id + ".Part" + base.Index + ".Failed";
        public List<string> outSignalsFailed = new List<string>();
        private readonly List<Pawn> culpritsResult = new List<Pawn>();
        public override AlertReport AlertReport {
            get {
                if (!showAlert || minTicksBelowThreshold < 60) {
                    return AlertReport.Inactive;
                }
                culpritsResult.Clear();
                if (MoodBelowThreshold(contract.tenant)) {
                    culpritsResult.Add(contract.tenant);
                }
                return AlertReport.CulpritsAre(culpritsResult);
            }
        }

        public override bool AlertCritical => true;

        public override string AlertLabel => Language.Translate.MoodBelowThreshold;

        public override string AlertExplanation => Language.Translate.MoodBelowThresholdDesc(contract.tenant);

        public override void QuestPartTick() {
            base.QuestPartTick();
            if (MoodBelowThreshold(contract.tenant)) {
                moodBelowThresholdTicks++;
                if (moodBelowThresholdTicks >= minTicksBelowThreshold) {
                    SignalArgs signalArgs = new SignalArgs(contract.tenant.Named("SUBJECT"));
                    Log.Message(signalArgs + " ");
                    Find.SignalManager.SendSignal(new Signal(OutSignalFailed, signalArgs));
                    for (int i = 0; i < outSignalsFailed.Count; i++) {
                        if (!outSignalsFailed[i].NullOrEmpty()) {
                            Find.SignalManager.SendSignal(new Signal(outSignalsFailed[i], signalArgs));
                        }
                    }
                }
            }
            else if (MoodAboveThreshold(contract.tenant)) {
                moodAboveThresholdTicks++;
                Log.Message(moodAboveThresholdTicks + " " + minTicksAboveThreshold);
                if (moodAboveThresholdTicks >= minTicksAboveThreshold) {
                    moodAboveThresholdTicks = 0;
                    SignalArgs signalArgs = new SignalArgs(contract.tenant.Named("SUBJECT"));
                    Log.Message(signalArgs + " ");
                    Find.SignalManager.SendSignal(new Signal(OutSignalCompleted, signalArgs));
                    for (int i = 0; i < outSignalsCompleted.Count; i++) {
                        if (!outSignalsCompleted[i].NullOrEmpty()) {
                            Find.SignalManager.SendSignal(new Signal(outSignalsCompleted[i], signalArgs));
                        }
                    }
                }
            }
            else {
                moodAboveThresholdTicks = 0;
                moodBelowThresholdTicks = 0;
            }
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref contract.tenant, "Pawn");
            Scribe_Values.Look(ref thresholdLow, "ThresholdLow", 0f);
            Scribe_Values.Look(ref thresholdHigh, "ThresholdHigh", 0f);
            Scribe_Values.Look(ref minTicksBelowThreshold, "MinTicksBelowThreshold", 0);
            Scribe_Values.Look(ref minTicksAboveThreshold, "MinTicksAboveThreshold", 0);
            Scribe_Values.Look(ref showAlert, "ShowAlert", defaultValue: true);
            Scribe_Values.Look(ref moodBelowThresholdTicks, "MoodBelowThresholdTicks", 0);
            Scribe_Values.Look(ref moodAboveThresholdTicks, "MoodAboveThresholdTicks", 0);
            Scribe_Collections.Look(ref outSignalsFailed, "OutSignalsFailed");
            Scribe_Deep.Look(ref contract, "Contract");
        }

        public override void AssignDebugData() {
            base.AssignDebugData();
            if (Find.AnyPlayerHomeMap != null) {
                Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
                contract.tenant = randomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault();
                thresholdLow = 0.25f;
                minTicksBelowThreshold = 2500;
            }
        }

        private bool MoodBelowThreshold(Pawn pawn) {
            if (pawn.needs == null || pawn.needs.mood == null) {
                return false;
            }
            return pawn.needs.mood.CurLevelPercentage < thresholdLow;
        }
        private bool MoodAboveThreshold(Pawn pawn) {
            if (pawn.needs == null || pawn.needs.mood == null) {
                return false;
            }
            return pawn.needs.mood.CurLevelPercentage > thresholdHigh;
        }
    }
}
