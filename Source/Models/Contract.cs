using RimWorld;
using System;
using Verse;

namespace Tenants.Models {
    public class Contract : IExposable {
        public Pawn _tenant;
        public int _length;
        public int _startdate;
        public int _endDate;
        public int _rent;
        public bool _singleRoomRequirement;
        //public RoomRequirement roomRequirement;
        public int LengthDays { get { return _length / 60000; } }

        public void ExposeData() {
            Scribe_Values.Look(ref _length, "length");
            Scribe_Values.Look(ref _startdate, "startdate");
            Scribe_Values.Look(ref _endDate, "endDate");
            Scribe_Values.Look(ref _rent, "rent");
            Scribe_Values.Look(ref _singleRoomRequirement, "SingleRoomRequirement");
            Scribe_References.Look(ref _tenant, "tenant");
        }
    }
}
