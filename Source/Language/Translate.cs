using Verse;

namespace Tenants.Language {
    public static class Translate {
	    //Courier
        public static string CourierDenied() => "LTSCourierDenied".Translate();
        public static string CourierDeniedMessage(Pawn pawn) => "LTSCourierDeniedMessage".Translate(pawn.Named("PAWN"));
        public static string CourierArrival() => "LTSCourierArrival".Translate();
        public static string CourierArrivalMessage(Pawn pawn) => "LTSCourierArrivalMessage".Translate(pawn.Named("PAWN"));
        public static string CourierDelivered(Pawn pawn) => "LTSCourierDelivered".Translate(pawn.Named("PAWN"));
        public static string CourierDeliveredRent(Pawn pawn, int rent) => "LTSCourierDeliveredRent".Translate(rent, pawn.Named("PAWN"));
        public static string CourierDeliveredRentGold(Pawn pawn, int rent, int gold) => "LTSCourierDeliveredRentGold".Translate(rent, gold, pawn.Named("PAWN"));
        public static string CourierDeliveredGift() => "LTSCourierDeliveredGift".Translate();
        public static string CourierDeliveredTech() => "LTSCourierDeliveredTech".Translate();
        public static string CourierTenancyNotice(Pawn pawn) => "LTSCourierTenancyNotice".Translate(pawn.Named("PAWN"));
        public static string CourierTenancyNoticeFailFunds(Pawn pawn) => "LTSCourierTenancyNoticeFailFunds".Translate(pawn.Named("PAWN"));
        public static string AdvertisementPlaced() => "LTSAdvertisementPlaced".Translate();
        public static string AdvertisementFailed(Pawn pawn) => "LTSAdvertisementFailed".Translate(pawn.Named("PAWN"));
        public static string AdvertisementGizmo() => "LTSAdvertisementGizmo".Translate();
        public static string AdvertisementGizmoDesc() => "LTSAdvertisementGizmoDesc".Translate();
        public static string AdvertisementFemaleGizmo() => "LTSAdvertisementFemaleGizmo".Translate();
        public static string AdvertisementMaleGizmo() => "LTSAdvertisementMaleGizmo".Translate();
        public static string AdvertisementGenderGizmo() => "LTSAdvertisementGenderGizmo".Translate();
        public static string AdvertisementAge(int value) => "LTSAdvertisementAgeGizmo".Translate(value);
        public static string AdvertisementAnyAge() => "LTSAdvertisementAnyAgeGizmo".Translate();
        public static string AdvertisementSingleRoomGizmo() => "LTSAdvertisementSingleRoomGizmo".Translate();
        public static string AdvertisementSingleRoomGizmoDesc() => "LTSAdvertisementSingleRoomGizmoDesc".Translate();
        public static string AdvertisementFightableGizmo() => "LTSAdvertisementFightableGizmo".Translate();
        public static string AdvertisementFightableGizmoDesc() => "LTSAdvertisementFightableGizmoDesc".Translate();
        public static string AdvertisementMayJoinGizmo() => "LTSAdvertisementMayJoinGizmo".Translate();
        public static string AdvertisementMayJoinGizmoDesc() => "LTSAdvertisementMayJoinGizmoDesc".Translate();
        public static string AdvertisementXenosAnyGizmo() => "LTSAdvertisementXenosAnyGizmo".Translate();
        public static string AdvertisementCost() => "LTSAdvertisementCost".Translate();
        public static string AdvertisementCostDesc() => "LTSAdvertisementCostDesc".Translate();
        public static string ExpectedRent(int value) => "LTSExpectedRent".Translate(value);
        public static string LoneCourierDied(Pawn pawn) => "LTSLoneCourierDied".Translate(pawn.Named("PAWN"));
        //Tenancy
        public static string ContractText(Pawn pawn, int rent, int days) => "LTSContractText".Translate(rent, days, pawn.Named("PAWN"));
        public static string ContractJoin(Pawn pawn) => "LTSContractJoin".Translate(pawn.Named("PAWN"));
        public static string ContractJoinReject(Pawn pawn) => "LTSContractJoinReject".Translate(pawn.Named("PAWN"));
        public static string ContractJoinAccept(Pawn pawn) => "LTSContractJoinAccept".Translate(pawn.Named("PAWN"));
        public static string ContractTitle() => "LTSContractTitle".Translate();
        public static string ContractAgree() => "LTSContractAgree".Translate();
        public static string ContractReject() => "LTSContractReject".Translate();
        public static string ContractPostpone() => "LTSContractPostpone".Translate();
        public static string MoodBelowThreshold() => "LTSMoodBelowThreshold".Translate();
        public static string MoodBelowThresholdDesc(Pawn pawn) => "LTSMoodBelowThresholdDesc".Translate(pawn.Named("PAWN"));
        public static string TenantPassionMinor() => "LTSTenantPassionMinor".Translate();
        public static string TenantPassionMajor() => "LTSTenantPassionMajor".Translate();
        public static string TenancyRoomRequired() => "LTSTenancyRoomRequired".Translate();
        public static string TenancyRoomRequiredDesc() => "LTSTenancyRoomRequiredDesc".Translate();
        public static string LoneTenantDied(Pawn pawn) => "LTSLoneTenantDied".Translate(pawn.Named("PAWN"));
        //Settings
        public static string CourierDaysSpawn(int min, int max) => "LTSCourierDaysSpawn".Translate(min,max);
        public static string CourierDaysSpawnDesc() => "LTSCourierDaysSpawnDesc".Translate();
        public static string TenancyDaysContract(int min, int max) => "LTSTenancyDaysContract".Translate(min,max);
        public static string TenancyRentContract() => "LTSTenancyRentContract".Translate();
        public static string TenancyRentContractDesc() => "LTSTenancyRentContractDesc".Translate();
        public static string MoodTicks() => "LTSMoodTicks".Translate();
        public static string MoodTicksDesc() => "LTSMoodTicksDesc".Translate();
        public static string KillPenalty() => "LTSKillPenalty".Translate();
        public static string KillPenaltyDesc() => "LTSKillPenaltyDesc".Translate();
        public static string GoldPayment() => "LTSGoldPayment".Translate();
        public static string GoldPaymentDesc() => "LTSGoldPaymentDesc".Translate();
        public static string AdvertNoticeSound() => "LTSAdvertNoticeSound".Translate();
        public static string AdvertNoticeSoundDesc() => "LTSAdvertNoticeSoundDesc".Translate();
    }
}
