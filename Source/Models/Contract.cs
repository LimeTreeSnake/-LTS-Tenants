using Verse;

namespace Tenants.Models {
    public class Contract : IExposable {
        public Pawn _tenant;
        public int _length;
        public int _startDate;
        public int _endDate;
        public int _rent;
        public bool _mayJoin;
        public bool _singleRoomRequirement;
        public bool _violenceEnabled = true;
        //public RoomRequirement roomRequirement;
        public int LengthDays { get { return _length / 60000; } }

        public void ExposeData() {
            Scribe_Values.Look(ref _length, "length");
            Scribe_Values.Look(ref _startDate, "startDate");
            Scribe_Values.Look(ref _endDate, "endDate");
            Scribe_Values.Look(ref _rent, "rent");
            Scribe_Values.Look(ref _mayJoin, "MayJoin");
            Scribe_Values.Look(ref _singleRoomRequirement, "SingleRoomRequirement");
            Scribe_Values.Look(ref _violenceEnabled, "ViolenceEnabled");
            Scribe_References.Look(ref _tenant, "tenant");
        }
    }
}
