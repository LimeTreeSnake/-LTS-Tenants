using Verse;

namespace Tenants.Models {
    public class Contract : IExposable {
        public Pawn tenant;
        public int length;
        public int startdate;
        public int endDate;
        public int rent;
        public int LengthDays { get { return length / 60000; } }

        public void ExposeData() {
            Scribe_Values.Look<int>(ref length, "length");
            Scribe_Values.Look<int>(ref startdate, "startdate");
            Scribe_Values.Look<int>(ref endDate, "endDate");
            Scribe_Values.Look<int>(ref rent, "rent");
            Scribe_References.Look<Pawn>(ref tenant, "tenant");
        }
    }
}
