using RimWorld;
using Verse;

namespace Tenants.Models {
    public class Contract : IExposable {
        public Pawn tenant;
        public int length;
        public int startdate;
        public int endDate;
        public int rent;
        public bool singleRoomRequirement;
        //public RoomRequirement roomRequirement;
        public int LengthDays { get { return length / 60000; } }

        public void ExposeData() {
            Scribe_Values.Look(ref length, "length");
            Scribe_Values.Look(ref startdate, "startdate");
            Scribe_Values.Look(ref endDate, "endDate");
            Scribe_Values.Look(ref rent, "rent");
            Scribe_Values.Look(ref singleRoomRequirement, "SingleRoomRequirement");
            Scribe_References.Look(ref tenant, "tenant");
        }
    }
}
