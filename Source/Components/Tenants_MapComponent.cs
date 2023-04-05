using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using System.Linq;
using RimWorld.QuestGen;
using Tenants.Models;
using Tenants.Things;

namespace Tenants.Components
{
	public class Tenants_MapComponent : MapComponent
	{

		#region Fields

		private NoticeBoard _noticeBoard;
		private int _courierFireTick, _tenantFireTick;
		private int _tenantKills, _courierKills, _silver;
		private List<Pawn> _tenantsPool = new List<Pawn>();
		private List<Pawn> _courierPool = new List<Pawn>();
		private List<Contract> _activeContracts = new List<Contract>();
		private bool _tenantAddUpp, _courierIsFiring;

		#endregion Fields

		public NoticeBoard NoticeBoard => _noticeBoard;

		public int TenantKills
		{
			get => _tenantKills;
			set => _tenantKills = value;
		}

		public int CourierKills
		{
			get => _courierKills;
			set => _courierKills = value;
		}

		public List<Contract> ActiveContracts
		{
			get
			{
				return _activeContracts ?? (_activeContracts = new List<Contract>());
			}
		}

		public Tenants_MapComponent(Map map)
			: base(map)
		{
		}

		#region Methods

		public bool IsTenant(Pawn pawn)
		{
			return _tenantsPool.Contains(pawn);
		}

		public void RemoveTenant(Pawn pawn)
		{
			_tenantsPool.Remove(pawn);
		}

		public bool IsCourier(Pawn pawn)
		{
			return _courierPool.Contains(pawn);
		}

		public bool IsContractedTenant(Pawn p, out Contract cont)
		{
			cont = null;
			if (!IsTenant(p))
			{
				return false;
			}

			if (!p.HasExtraHomeFaction() && p.IsFreeNonSlaveColonist)
			{
				_tenantsPool.Remove(p);
			}
			else
			{
				foreach (Contract t in _activeContracts.Where(t => t._tenant == p))
				{
					cont = t;
					return true;
				}
			}

			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref _noticeBoard, "NoticeBoard");
			Scribe_Values.Look(ref _courierFireTick, "CourierFireTick");
			Scribe_Values.Look(ref _tenantFireTick, "TenantFireTick");
			Scribe_Values.Look(ref _courierKills, "CourierKills");
			Scribe_Values.Look(ref _tenantKills, "TenantKills");
			Scribe_Values.Look(ref _silver, "Silver");
			Scribe_Collections.Look(ref _tenantsPool, "TenantsPool", LookMode.Reference);
			Scribe_Collections.Look(ref _courierPool, "CourierPool", LookMode.Reference);
			Scribe_Collections.Look(ref _activeContracts, "ActiveContracts", LookMode.Deep);
			Scribe_Values.Look(ref _tenantAddUpp, "TenantAddUpp");
			Scribe_Values.Look(ref _courierIsFiring, "CourierIsFiring");
		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();
			if (_noticeBoard == null)
			{
				return;
			}

			if (_tenantAddUpp)
			{
				_tenantFireTick--;
				if (_tenantFireTick <= 0)
				{
					var slate = new Slate();
					slate.Set<float>("points", Rand.Range(500, 10000));
					Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Defs.QuestDefOf.Tenancy, new Slate());
					QuestUtility.SendLetterQuestAvailable(quest);
				}
			}

			if (_noticeBoard == null || _courierIsFiring)
			{
				return;
			}

			_courierFireTick--;
			if (_courierFireTick > 0)
			{
				return;
			}

			Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.CourierArrival,
				Find.TickManager.TicksGame + 10000,
				StorytellerUtility.DefaultParmsNow(Defs.IncidentDefOf.CourierArrival.category, this.map),
				240000);

			_courierIsFiring = true;
		}

		public Pawn GetCourier()
		{
			try
			{
				Faction faction = Find.FactionManager.FirstFactionOfDef(Defs.FactionDefOf.LTS_Courier);
				if (faction == null)
				{
					faction = FactionGenerator.NewGeneratedFaction(
						new FactionGeneratorParms(Defs.FactionDefOf.LTS_Courier));

					Find.FactionManager.Add(faction);
				}

				if (_courierPool == null)
				{
					_courierPool = new List<Pawn>();
				}

				if (_courierPool.Count > 0)
				{
					{
						for (int i = 0; i < _courierPool.Count; i++)
						{
							if (_courierPool[i].DestroyedOrNull() || _courierPool[i].Dead)
							{
								_courierPool.RemoveAt(i);
								i--;
							}
							else if (_courierPool[i].Faction == Faction.OfPlayer || _courierPool[i].IsPrisoner)
							{
								_courierPool.RemoveAt(i);
								_courierKills++;
								i--;
							}
						}
					}
				}

				if (Settings.Settings.CourierDays.min == 0)
				{
					Settings.Settings.Reset();
					Log.Warning("Tenancy settings had no min/max courier spawn days set, mod settings got reset!");
				}

				_courierFireTick = Rand.RangeInclusive(Settings.Settings.CourierDays.min * 60000,
					Settings.Settings.CourierDays.max * 60000);

				_courierIsFiring = false;
				int tries = 0;
				while (_courierPool.Count(x => x.Spawned == false) < 6 && tries < 100)
				{
					tries++;
					PawnKindDef courierDef = Settings.Settings.GetCourierByWeight;
					Pawn newCourier = PawnGenerator.GeneratePawn(courierDef, faction);
					if (newCourier == null ||
					    newCourier.Dead ||
					    newCourier.IsDessicated() ||
					    newCourier.AnimalOrWildMan())
					{
						continue;
					}

					PawnGenerator.RedressPawn(newCourier, new PawnGenerationRequest(courierDef, faction));
					_courierPool.Add(newCourier);
					newCourier.DestroyOrPassToWorld();
				}

				return _courierPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer).RandomElement();
			}
			catch (Exception ex)
			{
				Log.Error("Error at GetCourier method: " + ex.Message);
				return null;
			}
		}

		public Pawn GetTenant()
		{
			try
			{
				Pawn tenant = null;
				Faction faction = Find.FactionManager.FirstFactionOfDef(Defs.FactionDefOf.LTS_Tenant);
				if (faction == null)
				{
					faction = FactionGenerator.NewGeneratedFaction(
						new FactionGeneratorParms(Defs.FactionDefOf.LTS_Tenant));

					Find.FactionManager.Add(faction);
				}

				_tenantAddUpp = false;
				if (_activeContracts == null)
				{
					_activeContracts = new List<Contract>();
				}

				for (int i = 0; i < _activeContracts.Count; i++)
				{
					if (_activeContracts[i]._tenant == null || !_activeContracts[i]._tenant.Spawned)
					{
						_activeContracts.RemoveAt(i);
						i--;
					}
				}

				if (_tenantsPool == null)
				{
					_tenantsPool = new List<Pawn>();
				}

				for (int i = 0; i < _tenantsPool.Count; i++)
				{
					if (_tenantsPool[i].DestroyedOrNull() || _tenantsPool[i].Dead)
					{
						_tenantsPool.RemoveAt(i);
						i--;
					}
				}

				_tenantFireTick = 0;
				while (_tenantsPool.Count(x => x.Spawned == false && x.Faction != Faction.OfPlayer) <
				       Settings.Settings.WorldTenants)
				{
					if (Settings.Settings.AvailableRaces != null && Settings.Settings.AvailableRaces.Count > 0)
					{
						string race = Settings.Settings.AvailableRaces?.RandomElement();
						if (race == null)
						{
							continue;
						}

						PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x =>
								x.race.defName.Contains(race) && x.combatPower < 50 && !x.factionLeader)
							.RandomElement();

						if (random != null)
						{
							Pawn newTenant = PawnGenerator.GeneratePawn(random, faction);
							if (newTenant == null || newTenant.AnimalOrWildMan())
							{
								continue;
							}

							newTenant.DestroyOrPassToWorld();
							_tenantsPool.Add(newTenant);
						}
						else
						{
							Settings.Settings.AvailableRaces.Remove(race);
							Log.Message("Removed (" +
							            race +
							            ") as an available race since they don't seem to have any plausible tenants.");
						}
					}
					else
					{
						Log.Error("No pawns available for tenancy, check your Tenants settings!");
						return null;
					}
				}

				if (_tenantsPool.Count > 0)
				{
					tenant = _tenantsPool.Where(x => x.Spawned == false && x.Faction != Faction.OfPlayer)
						.RandomElement();
				}

				if (tenant == null)
				{
					return null;
				}

				foreach (Need t in tenant.needs.AllNeeds)
				{
					t.CurLevelPercentage = 0.6f;
				}

				return tenant;
			}
			catch (Exception ex)
			{
				Log.Error("Error at GetTenant method: " + ex.Message);
				return null;
			}
		}

		public void EmptyBoard(Pawn courier)
		{
			try
			{
				if (_noticeBoard == null)
				{
					return;
				}

				if (_silver > 0)
				{
					if (ModLister.RoyaltyInstalled &&
					    _silver > 1000 &&
					    !Defs.ResearchDefOf.LTS_CourierTech.IsFinished &&
					    Defs.ResearchDefOf.LTS_CourierTech.TechprintsApplied <
					    Defs.ResearchDefOf.LTS_CourierTech.TechprintCount)
					{
						Messages.Message(Language.Translate.CourierDeliveredTech, _noticeBoard,
							MessageTypeDefOf.PositiveEvent);

						Find.ResearchManager.ApplyTechprint(Defs.ResearchDefOf.LTS_CourierTech, null);
					}

					if (Settings.Settings.PaymentGold && _silver > 500)
					{
						float percentage = Rand.Range(0.1f, 0.20f);
						double gold = (_silver / 10.0) * percentage;
						_silver -= (int)(_silver * percentage);
						if (gold < 1)
						{
							gold = 1;
						}

						DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, _noticeBoard.Position, _silver);
						DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Gold, _noticeBoard.Position, (int)gold);
						Messages.Message(Language.Translate.CourierDeliveredRentGold(courier, _silver, (int)gold),
							_noticeBoard, MessageTypeDefOf.NeutralEvent);
					}
					else
					{
						DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, _noticeBoard.Position, _silver);
						Messages.Message(Language.Translate.CourierDeliveredRent(courier, _silver), _noticeBoard,
							MessageTypeDefOf.NeutralEvent);
					}

					_silver = 0;
				}

				if (NoticeBoard != null)
				{
					if (NoticeBoard._noticeUp && _tenantAddUpp != true)
					{
						Messages.Message(Language.Translate.CourierTenancyNotice(courier),
							MessageTypeDefOf.PositiveEvent);

						NoticeBoard._noticeUp = false;
						_tenantAddUpp = true;
						_tenantFireTick = Rand.RangeInclusive(60000, 300000);
					}
					else if (_tenantAddUpp)
					{
						Messages.Message(Language.Translate.CourierTenancyNoticeFail(courier),
							MessageTypeDefOf.NeutralEvent);
					}
				}
				else
				{
					Log.Error(
						"Somehow the NoticeBoardComponent does not exist! Contact the author if you think it's something faulty with this code. " +
						_noticeBoard);
				}

				Messages.Message(Language.Translate.CourierDelivered(courier), _noticeBoard,
					MessageTypeDefOf.NeutralEvent);

			}
			catch (Exception ex)
			{
				Log.Message("Error at EmptyBoard method: " + ex.Message);
			}
		}

		public void Payday(Contract contract)
		{
			_silver += contract._rent * contract.LengthDays;
		}

		public bool FindNoticeBoardInMap()
		{
			int count = 0;
			_noticeBoard = null;
			foreach (NoticeBoard current in this.map.listerBuildings.allBuildingsColonist.OfType<NoticeBoard>())
			{
				if (count > 1)
				{
					_noticeBoard?.Destroy();
					_noticeBoard = current;
					Messages.Message(Language.Translate.MultipleNoticeBoards, MessageTypeDefOf.NeutralEvent);
				}
				else
				{
					_noticeBoard = current;
					count++;
				}
			}

			return _noticeBoard != null;
		}

		#endregion Methods

	}
}