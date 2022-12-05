using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using System.Linq;
using RimWorld.QuestGen;
using Tenants.Things;

namespace Tenants.Components {
    public class Tenants_MapComponent : MapComponent {
        #region Fields
        private NoticeBoard noticeBoard;
        private int courierFireTick = 0, tenantFireTick = 0;
        private int tenantKills = 0, courierKills = 0, silver = 0;
        private List<Pawn> tenantsPool = new List<Pawn>();
        private List<Pawn> courierPool = new List<Pawn>();
        private List<Models.Contract> activeContracts = new List<Models.Contract>();
        private bool tenantAddUpp = false, courierIsFiring = false, deliverGift = false;
        #endregion Fields

        public NoticeBoard NoticeBoard => noticeBoard;
        public int TenantFireTick {
            get => tenantFireTick;
            set => tenantFireTick = value;
        }
        public int CourierFireTick {
            get => courierFireTick;
            set => courierFireTick = value;
        }
        public int TenantKills {
            get => tenantKills;
            set => tenantKills = value;
        }
        public int CourierKills {
            get => courierKills;
            set => courierKills = value;
        }
        public List<Models.Contract> ActiveContracts {
            get {
                if (activeContracts == null) {
                    activeContracts = new List<Models.Contract>();
                }
                return activeContracts;
            }
        }

        public Tenants_MapComponent(Map map)
           : base(map) {
        }

        #region Methods
        public bool IsTenant(Pawn pawn) {
            return tenantsPool.Contains(pawn);
        }
        public bool RemoveTenant(Pawn pawn) {
            return tenantsPool.Remove(pawn);
        }
        public bool IsCourier(Pawn pawn) {
            return courierPool.Contains(pawn);
        }
        public bool IsContractedTenant(Pawn p, out Models.Contract cont) {
            cont = null;
            if (IsTenant(p)) {
                if (!p.HasExtraHomeFaction() && p.IsFreeNonSlaveColonist) {
                    tenantsPool.Remove(p);
                }
                else {
                    for (int i = 0; i < activeContracts.Count; i++) {
                        if (activeContracts[i].tenant == p) {
                            cont = activeContracts[i];
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref noticeBoard, "NoticeBoard");
            Scribe_Values.Look(ref courierFireTick, "CourierFireTick", 0);
            Scribe_Values.Look(ref tenantFireTick, "TenantFireTick", 0);
            Scribe_Values.Look(ref courierKills, "CourierKills", 0);
            Scribe_Values.Look(ref tenantKills, "TenantKills", 0);
            Scribe_Values.Look(ref silver, "Silver", 0);
            Scribe_Collections.Look(ref tenantsPool, "TenantsPool", LookMode.Reference);
            Scribe_Collections.Look(ref courierPool, "CourierPool", LookMode.Reference);
            Scribe_Collections.Look(ref activeContracts, "ActiveContracts", LookMode.Deep);
            Scribe_Values.Look(ref tenantAddUpp, "TenantAddUpp", false);
            Scribe_Values.Look(ref courierIsFiring, "CourierIsFiring", false);
        }
        public override void MapComponentTick() {
            base.MapComponentTick();
            if (noticeBoard != null) {
                if (tenantAddUpp) {
                    tenantFireTick--;
                    if (tenantFireTick <= 0) {
                        Slate slate = new Slate();
                        slate.Set<float>("points", Rand.Range(500, 10000), false);
                        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Defs.QuestDefOf.Tenancy, new Slate());
                        QuestUtility.SendLetterQuestAvailable(quest);
                    }
                }
                if (noticeBoard != null && !courierIsFiring) {
                    courierFireTick--;
                    if (courierFireTick <= 0) {
                        Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.CourierArrival, Find.TickManager.TicksGame + 10000, StorytellerUtility.DefaultParmsNow(Defs.IncidentDefOf.CourierArrival.category, this.map), 240000);
                        courierIsFiring = true;
                    }
                }
            }
        }
        public Pawn GetCourier() {
            try {
                Pawn courier = null;
                Faction faction = Find.FactionManager.FirstFactionOfDef(Defs.FactionDefOf.LTS_Courier);
                if (faction == null) {
                    faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(Defs.FactionDefOf.LTS_Courier, default, null));
                    Find.FactionManager.Add(faction);
                }
                if (courierPool == null) {
                    courierPool = new List<Pawn>();
                }
                if (courierPool.Count > 0) {
                    {
                        for (int i = 0; i < courierPool.Count; i++) {
                            if (courierPool[i].DestroyedOrNull() || courierPool[i].Dead) {
                                courierPool.RemoveAt(i);
                                i--;
                            }
                            else if (courierPool[i].Faction == Faction.OfPlayer || courierPool[i].IsPrisoner) {
                                courierPool.RemoveAt(i);
                                courierKills++;
                                i--;
                            }
                        }
                    }
                }
                if (Settings.Settings.CourierDays.min == 0) {
                    Settings.Settings.Reset();
                    Log.Warning("Tenany settings had no min/max courier spawn days set, mod settings got reset!");
                }
                courierFireTick = Rand.RangeInclusive(Settings.Settings.CourierDays.min * 60000, Settings.Settings.CourierDays.max * 60000);
                courierIsFiring = false;
                while (courierPool.Count < 6) {
                    PawnKindDef courierDef = Settings.Settings.GetCourierByWeight;
                    Pawn newCourier = PawnGenerator.GeneratePawn(courierDef, faction);
                    if (newCourier != null && !newCourier.Dead && !newCourier.IsDessicated() && !newCourier.AnimalOrWildMan()) {
                        newCourier.DestroyOrPassToWorld();
                        if (!newCourier.apparel.AnyApparel) {
                            PawnGenerator.RedressPawn(newCourier, new PawnGenerationRequest(courierDef, faction));
                        }
                        courierPool.Add(newCourier);
                    }
                }
                if (courierPool.Count > 0) {
                    courier = courierPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer)?.RandomElement();
                }
                return courierPool.RandomElement();
            }
            catch (Exception ex) {
                Log.Error("Error at GetCourier method: " + ex.Message);
                return null;
            }
        }
        public Pawn GetTenant() {
            try {
                Pawn tenant = null;
                Faction faction = Find.FactionManager.FirstFactionOfDef(Defs.FactionDefOf.LTS_Tenant);
                if (faction == null) {
                    faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(Defs.FactionDefOf.LTS_Tenant, default, null));
                    Find.FactionManager.Add(faction);
                }
                tenantAddUpp = false;
                if (activeContracts == null) {
                    activeContracts = new List<Models.Contract>();
                }
                for (int i = 0; i < activeContracts.Count; i++) {
                    if (activeContracts[i].tenant == null || !activeContracts[i].tenant.Spawned) {
                        activeContracts.RemoveAt(i);
                        i--;
                    }
                }
                if (tenantsPool == null) {
                    tenantsPool = new List<Pawn>();
                }
                for (int i = 0; i < tenantsPool.Count; i++) {
                    if (tenantsPool[i].DestroyedOrNull() || tenantsPool[i].Dead) {
                        tenantsPool.RemoveAt(i);
                        i--;
                    }
                }
                tenantFireTick = 0;
                while (tenantsPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer).Count() < Settings.Settings.WorldTenants) {
                    if (Settings.Settings.AvailableRaces.Count > 0) {
                        string race;
                        PawnKindDef random = null;
                        race = Settings.Settings.AvailableRaces?.RandomElement();
                        if (race != null) {
                            random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName.Contains(race) && x.combatPower < 50 && !x.factionLeader)?.RandomElement();
                            if (random != null) {
                                Pawn newTenant = PawnGenerator.GeneratePawn(random, faction);
                                if (newTenant != null && !newTenant.AnimalOrWildMan()) {
                                    newTenant.DestroyOrPassToWorld();
                                    tenantsPool.Add(newTenant);
                                }
                            }
                            else {
                                Settings.Settings.AvailableRaces.Remove(race);
                                Log.Message("Removed (" + race + ") as an available race since they don't seem to have any plausible tenants.");
                            }
                        }
                    }
                    else {
                        Log.Error("No pawns available for tenancy, check your Tenants settings!");
                        return null;
                    }
                }
                if (tenantsPool.Count > 0) {
                    tenant = tenantsPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer)?.RandomElement();
                }
                for (int i = 0; i < tenant.needs.AllNeeds.Count; i++) {
                    tenant.needs.AllNeeds[i].CurLevelPercentage = 0.6f;
                }
                return tenant;
            }
            catch (Exception ex) {
                Log.Error("Error at GetTenant method: " + ex.Message);
                return null;
            }
        }
        public void EmptyBoard(Pawn courier) {
            try {
                if (noticeBoard == null) {
                    return;
                }
                if (silver > 0) {
                    if (ModLister.RoyaltyInstalled && silver > 1000 && !Defs.ResearchDefOf.LTS_CourierTech.IsFinished && Defs.ResearchDefOf.LTS_CourierTech.TechprintsApplied < Defs.ResearchDefOf.LTS_CourierTech.TechprintCount) {
                        Messages.Message(Language.Translate.CourierDeliveredTech, noticeBoard, MessageTypeDefOf.PositiveEvent);
                        Find.ResearchManager.ApplyTechprint(Defs.ResearchDefOf.LTS_CourierTech, null);
                    }
                    if (Settings.Settings.PaymentGold && silver > 500) {
                        float percentage = Rand.Range(0.1f, 0.20f);
                        int gold = (int)((float)(silver / 10) * percentage);
                        silver = silver - (int)(silver * percentage);
                        if(gold < 1) {
                            gold = 1;
                        }
                        DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, noticeBoard.Position, silver);
                        DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Gold, noticeBoard.Position, gold);
                        Messages.Message(Language.Translate.CourierDeliveredRentGold(courier, silver, gold), noticeBoard, MessageTypeDefOf.NeutralEvent);
                    }
                    else {
                        DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, noticeBoard.Position, silver);
                        Messages.Message(Language.Translate.CourierDeliveredRent(courier, silver), noticeBoard, MessageTypeDefOf.NeutralEvent);
                    }
                    silver = 0;
                }
                if (NoticeBoard != null) {
                    if (NoticeBoard.noticeUp && tenantAddUpp != true) {
                        Messages.Message(Language.Translate.CourierTenancyNotice(courier), MessageTypeDefOf.PositiveEvent);
                        NoticeBoard.noticeUp = false;
                        tenantAddUpp = true;
                        tenantFireTick = Rand.RangeInclusive(60000, 300000);
                    }
                    else if (tenantAddUpp) {
                        Messages.Message(Language.Translate.CourierTenancyNoticeFail(courier), MessageTypeDefOf.NeutralEvent);
                    }
                }
                else {
                    Log.Error("Somehow the NoticeBoardComponent does not exist! Contact the author if you think it's something faulty with this code. " + noticeBoard);
                }
                Messages.Message(Language.Translate.CourierDelivered(courier), noticeBoard, MessageTypeDefOf.NeutralEvent);

            }
            catch (System.Exception ex) {
                Log.Message("Error at EmptyBoard method: " + ex.Message);
            }
        }
        public void Payday(Models.Contract contract) {
            silver += contract.rent * contract.LengthDays;
        }
        public bool FindNoticeBoardInMap() {
            int count = 0;
            noticeBoard = null;
            foreach (Building current in this.map.listerBuildings.allBuildingsColonist) {
                if (current is NoticeBoard) {
                    if (count > 1) {
                        noticeBoard.Destroy();
                        noticeBoard = current as NoticeBoard;
                        Messages.Message(Language.Translate.MultipleNoticeBoards, MessageTypeDefOf.NeutralEvent);
                        continue;
                    }
                    else {
                        noticeBoard = current as NoticeBoard;
                        count++;
                    }
                }
            }
            return noticeBoard != null;
        }
        #endregion Methods
    }
}
