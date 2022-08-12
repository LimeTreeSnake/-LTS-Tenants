using Verse;

namespace Tenants.Language {
    public static class Translate {
        public static string MultipleNoticeBoards => "MultipleNoticeBoards".Translate();
        public static string CourierDenied => "CourierDenied".Translate();
        public static string CourierDeniedMessage(Pawn pawn) => "CourierDeniedMessage".Translate(pawn.Named("PAWN"));
        public static string CourierArrival => "CourierArrival".Translate();
        public static string CourierArrivalMessage(Pawn pawn) => "CourierArrivalMessage".Translate(pawn.Named("PAWN"));
        public static string CourierDelivered(Pawn pawn) => "CourierDelivered".Translate(pawn.Named("PAWN"));
        public static string CourierDeliveredRent(Pawn pawn, int rent) => "CourierDeliveredRent".Translate(rent, pawn.Named("PAWN"));
        public static string CourierTenancyNotice(Pawn pawn) => "CourierTenancyNotice".Translate(pawn.Named("PAWN"));
        public static string CourierTenancyNoticeFail(Pawn pawn) => "CourierTenancyNoticeFail".Translate(pawn.Named("PAWN"));
        public static string AddNoticeForTenancy(int cost) => "AddNoticeForTenancy".Translate(cost);
        public static string AdvertisementPlaced => "AdvertisementPlaced".Translate();


        public static string TenancyDenied => "TenancyDenied".Translate();
        public static string TenancyDeniedMessage(Pawn pawn) => "TenancyDeniedMessage".Translate(pawn.Named("PAWN"));
        public static string ContractText(Pawn pawn, int rent, int days) => "ContractText".Translate(rent, days, pawn.Named("PAWN"));
        public static string ContractJoin(Pawn pawn) => "ContractJoin".Translate(pawn.Named("PAWN"));
        public static string ContractJoinReject(Pawn pawn) => "ContractJoinReject".Translate(pawn.Named("PAWN"));
        public static string ContractJoinAccept(Pawn pawn) => "ContractJoinAccept".Translate(pawn.Named("PAWN"));
        public static string ContractTitle => "ContractTitle".Translate();
        public static string ContractAgree => "ContractAgree".Translate();
        public static string ContractReject => "ContractReject".Translate();
        public static string MoodBelowThreshold => "MoodBelowThreshold".Translate();
        public static string MoodBelowThresholdDesc(Pawn pawn) => "MoodBelowThresholdDesc".Translate(pawn.Named("PAWN")); 
        public static string TenancyRoomRequired => "TenancyRoomRequired".Translate();
        public static string TenancyRoomRequiredDesc => "TenancyRoomRequiredDesc".Translate();

        //Settings
        public static string Races => "Races".Translate();
        public static string Filter => "Filter".Translate();
        public static string DefaultSettings => "DefaultSettings".Translate();
        public static string TenancyDaysContract(int min, int max) => "TenancyDaysContract".Translate(min, max);
        public static string TenancyRentContract(int rent) => "TenancyRentContract".Translate(rent);
        public static string AdvertisementCost(int value) => "AdvertisementCost".Translate(value);
        public static string AdvertisementCostDesc() => "AdvertisementCostDesc".Translate();
        public static string MoodTicks(int value) => "MoodTicks".Translate(value);
        public static string MoodTicksDesc => "MoodTicksDesc".Translate();
        public static string KillPenalty => "KillPenalty".Translate();
        public static string KillPenaltyDesc => "KillPenaltyDesc".Translate();


    }
}
