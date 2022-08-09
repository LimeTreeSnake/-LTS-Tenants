using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using System.Linq;
using RimWorld.QuestGen;

namespace Tenants.Components {
    public class Tenants_MapComponent : MapComponent {
        #region Fields
        private Thing noticeBoard;
        private int courierFireTick = 0, tenantFireTick = 0;
        private NoticeBoard_Component noticeBoardComp;
        private int tenantKills = 0, courierKills = 0, silver = 0;
        private List<Pawn> tenantsPool = new List<Pawn>();
        private List<Pawn> courierPool = new List<Pawn>();
        private List<Pawn> activeTenants = new List<Pawn>();
        private bool tenantAddUpp = false, courierIsFiring = false;
        #endregion Fields

        public Thing NoticeBoard => noticeBoard;
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
        public Tenants_MapComponent(Map map)
           : base(map) {
        }
        public Tenants_MapComponent(bool generateComponent, Map map)
            : base(map) {
            if (generateComponent) {
                map.components.Add(this);
            }
        }

        #region Methods
        public static Tenants_MapComponent GetComponent(Map map) {
            return map.GetComponent<Tenants_MapComponent>() ?? new Tenants_MapComponent(generateComponent: true, map);
        }
        public bool IsTenant(Pawn pawn) {
            return tenantsPool.Contains(pawn);
        }
        public bool IsCourier(Pawn pawn) {
            return courierPool.Contains(pawn);
        }
        public override void ExposeData() {
            Scribe_Collections.Look(ref tenantsPool, "TenantsPool", LookMode.Reference);
            Scribe_Collections.Look(ref courierPool, "CourierPool", LookMode.Reference);
            Scribe_Collections.Look(ref activeTenants, "ActiveTenants", LookMode.Reference);
            Scribe_References.Look(ref noticeBoard, "NoticeBoard");
            Scribe_Values.Look(ref courierKills, "CourierKills", 0);
            Scribe_Values.Look(ref tenantKills, "TenantKills", 0);
            Scribe_Values.Look(ref silver, "Silver", 0);
            Scribe_Values.Look(ref tenantAddUpp, "TenantAddUpp", false);
        }
        #endregion Methods
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
                        Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.CourierArrival, Find.TickManager.TicksGame + Rand.Range(15000, 120000), StorytellerUtility.DefaultParmsNow(Defs.IncidentDefOf.CourierArrival.category, this.map), 240000);
                        courierIsFiring = true;
                    }
                }
            }
        }
        public override void MapGenerated() {
            base.MapGenerated();
            if (noticeBoard == null) {
                FindNoticeBoardInMap();
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
                            if (courierPool[i].DestroyedOrNull() || courierPool[i].Dead ) {
                                courierPool.RemoveAt(i);
                                i--;
                            }
                            else if ( courierPool[i].Faction == Faction.OfPlayer || courierPool[i].IsPrisoner) {
                                courierPool.RemoveAt(i);
                                courierKills++;
                                i--;
                            }
                        }
                    }
                }
                courierFireTick = Rand.RangeInclusive(500000, 1500000);
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
                if (tenantAddUpp) {
                    tenantAddUpp = false;
                }
                if (tenantsPool == null) {
                    tenantsPool = new List<Pawn>();
                }
                if (tenantsPool.Count > 0) {
                    {
                        for (int i = 0; i < tenantsPool.Count; i++) {
                            if (tenantsPool[i].DestroyedOrNull() || tenantsPool[i].Dead) {
                                tenantsPool.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                tenantFireTick = 0;
                while (tenantsPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer).Count() < 6) {
                    if (Settings.Settings.AvailableRaces.Count > 0) {
                        string race;
                        PawnKindDef random = null;
                        race = Settings.Settings.AvailableRaces?.RandomElement();
                        if (race != null) {
                            random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race && x.combatPower < 50)?.RandomElement();
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
                    FindNoticeBoardInMap();
                }
                if (noticeBoardComp == null || noticeBoardComp != noticeBoard.TryGetComp<NoticeBoard_Component>()) {
                    noticeBoardComp = noticeBoard.TryGetComp<NoticeBoard_Component>();
                }
                if (silver > 0) {
                    Messages.Message(Language.Translate.CourierDeliveredRent(courier, silver), noticeBoard, MessageTypeDefOf.NeutralEvent);
                    DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, noticeBoard.Position, silver);
                    silver = 0;
                }
                if (noticeBoardComp != null) {
                    if (noticeBoardComp.noticeForTenancy && tenantAddUpp != true) {
                        Messages.Message(Language.Translate.CourierTenancyNotice(courier), MessageTypeDefOf.PositiveEvent);
                        noticeBoardComp.noticeForTenancy = false;
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
        public void FindNoticeBoardInMap() {
            int count = 0;
            noticeBoard = null;
            foreach (Building current in this.map.listerBuildings.allBuildingsColonist) {
                if (current.def == Defs.ThingDefOf.LTS_NoticeBoard) {
                    if (count > 1) {
                        noticeBoard.Destroy();
                        noticeBoard = current;
                        Messages.Message(Language.Translate.MultipleNoticeBoards, MessageTypeDefOf.NeutralEvent);
                        continue;
                    }
                    else {
                        noticeBoard = current;
                        count++;
                    }
                }
            }
        }
    }
}
