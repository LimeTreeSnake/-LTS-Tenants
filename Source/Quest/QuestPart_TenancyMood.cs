using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Tenants.QuestNodes {
    public class QuestPart_TenancyMood : QuestPartActivable {

        public Models.Contract contract;
        private readonly List<Pawn> culpritsResult = new List<Pawn>();
        public string inSignalPostpone;             //If pawn join offer is postponed
        private int moodBelowThresholdTicks;
        private int moodAboveThresholdTicks;
        public int minTicksBelowThreshold;
        public int minTicksAboveThreshold;
        public bool offeredJoin = false;        
        public string OutSignalFailed => "Quest" + quest.id + ".Part" + base.Index + ".Failed";
        public List<string> outSignalsFailed = new List<string>();
        public bool showAlert = true; 
        public float thresholdLow;
        public float thresholdHigh;
        public override AlertReport AlertReport {
            get {
                if (!showAlert || minTicksBelowThreshold < 60) {
                    return AlertReport.Inactive;
                }
                culpritsResult.Clear();
                if (contract._tenant != null && MoodBelowThreshold(contract._tenant)) {
                    culpritsResult.Add(contract._tenant);
                }
                return AlertReport.CulpritsAre(culpritsResult);
            }
        }
        public override void Notify_QuestSignalReceived(Signal signal) {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == this.inSignalPostpone) {
                offeredJoin = false;
            }
        }

        public override bool AlertCritical => true;

        public override string AlertLabel => Language.Translate.MoodBelowThreshold;

        public override string AlertExplanation => Language.Translate.MoodBelowThresholdDesc(contract._tenant);

        public override void QuestPartTick() {
            base.QuestPartTick();
            if (MoodBelowThreshold(contract._tenant)) {
                moodBelowThresholdTicks++;
                if (moodBelowThresholdTicks >= minTicksBelowThreshold) {
                    var signalArgs = new SignalArgs(contract._tenant.Named("SUBJECT"));
                    Find.SignalManager.SendSignal(new Signal(OutSignalFailed, signalArgs));
                    for (int i = 0; i < outSignalsFailed.Count; i++) {
                        if (!outSignalsFailed[i].NullOrEmpty()) {
                            Find.SignalManager.SendSignal(new Signal(outSignalsFailed[i], signalArgs));
                        }
                    }
                }
            }
            else if (!offeredJoin && MoodAboveThreshold(contract._tenant)) {
                moodAboveThresholdTicks++;
                if (moodAboveThresholdTicks >= minTicksAboveThreshold) {
                    moodAboveThresholdTicks = 0;
                    offeredJoin = true;
                    var signalArgs = new SignalArgs(contract._tenant.Named("SUBJECT"));
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
            Scribe_Deep.Look(ref contract, "Contract");
            Scribe_Values.Look(ref inSignalPostpone, "InSignalPostpone", null, false);
            Scribe_Values.Look(ref moodBelowThresholdTicks, "MoodBelowThresholdTicks", 0);
            Scribe_Values.Look(ref moodAboveThresholdTicks, "MoodAboveThresholdTicks", 0);
            Scribe_Values.Look(ref minTicksBelowThreshold, "MinTicksBelowThreshold", 0);
            Scribe_Values.Look(ref minTicksAboveThreshold, "MinTicksAboveThreshold", 0);
            Scribe_Values.Look(ref offeredJoin, "OfferedJoin", defaultValue: false);
            Scribe_Collections.Look(ref outSignalsFailed, "OutSignalsFailed");
            Scribe_Values.Look(ref showAlert, "ShowAlert", defaultValue: true);
            Scribe_Values.Look(ref thresholdLow, "ThresholdLow", 0f);
            Scribe_Values.Look(ref thresholdHigh, "ThresholdHigh", 0f);
        }

        public override void AssignDebugData() {
            base.AssignDebugData();
            if (Find.AnyPlayerHomeMap != null) {
                Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
                contract._tenant = randomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault();
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
