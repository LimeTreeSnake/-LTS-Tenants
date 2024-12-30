using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using System.Linq;
using System.Reflection;
using RimWorld.QuestGen;
using Tenants.Logic;
using Tenants.Models;
using Tenants.Things;

namespace Tenants.Components
{
	public class TenantsMapComponent : MapComponent
	{
		#region Fields

		private NoticeBoard _noticeBoard;
		private int _courierFireTick;
		private int _tenantKills, _courierKills, _silver;
		private List<Pawn> _courierPool = new List<Pawn>();
		private bool _courierIsFiring;
		#endregion Fields

		public NoticeBoard NoticeBoard()
		{
			if (_noticeBoard == null)
			{
				FindNoticeBoardInMap();
			}
			return _noticeBoard;
		}

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

		public TenantsMapComponent(Map map)
			: base(map)
		{
		}

		#region Methods

		public override void FinalizeInit()
		{
			base.FinalizeInit();

			GenerateFaction(Defs.FactionDefOf.LTS_Courier);
			GenerateFaction(Defs.FactionDefOf.LTS_Tenant);
			// var field = typeof(Defs.FactionDefOf).GetFields(BindingFlags.Static | BindingFlags.Public);
		}

		public void GenerateFaction(FactionDef def)
		{
			if (def == null)
			{
				return;
			}
			
			Faction faction = Find.FactionManager.FirstFactionOfDef(def);

			
			if (faction != null)
			{
				Find.FactionManager.OfPlayer.SetRelationDirect(faction, FactionRelationKind.Neutral);
			}
			else
			{
				faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));
				Find.FactionManager.Add(faction);
				Find.FactionManager.OfPlayer.SetRelationDirect(faction, FactionRelationKind.Neutral);
			}
		}

		public bool IsCourier(Pawn pawn)
		{
			return _courierPool.Contains(pawn);
		}

		public bool HaveRent()
		{
			return _silver > 0;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref _noticeBoard, "NoticeBoard");
			Scribe_Values.Look(ref _courierFireTick, "CourierFireTick");
			Scribe_Values.Look(ref _courierKills, "CourierKills");
			Scribe_Values.Look(ref _tenantKills, "TenantKills");
			Scribe_Values.Look(ref _silver, "Silver");
			Scribe_Collections.Look(ref _courierPool, "CourierPool", LookMode.Reference);
			Scribe_Values.Look(ref _courierIsFiring, "CourierIsFiring");
		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();
			if (_noticeBoard == null)
			{
				return;
			}

			if (_courierIsFiring)
			{
				return;
			}

			if (!_noticeBoard._isActive && !HaveRent())
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

				_courierFireTick = Rand.RangeInclusive(Settings.Settings.CourierDays.min * 60000,
					Settings.Settings.CourierDays.max * 60000);

				_courierIsFiring = false;
				int tries = 0;
				while (_courierPool.Count(x => x.Spawned == false) < 6 && tries < 100)
				{
					tries++;
					PawnKindDef courierDef = CourierLogic.GetRandomPawnKindDef();
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
				Log.Error($"LTS_Tenants Error - GetCourier: {ex.Message}\n{ex.StackTrace}");
				return null;
			}
		}

		public void EmptyBoard(Pawn courier)
		{
			try
			{
				if (NoticeBoard() == null)
				{
					return;
				}

				//Payment
				if (HaveRent())
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
						float percentage = Rand.Range(0.25f, 0.20f);
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

				//Advertisement
				if (_noticeBoard._noticeUp)
				{
					int amount = _noticeBoard.AdvertisementCost();
					if (_noticeBoard._silverAmount == amount)
					{
						Messages.Message(Language.Translate.CourierTenancyNotice(courier),
							MessageTypeDefOf.PositiveEvent);

						_noticeBoard._noticeUp = false;
						_noticeBoard._silverAmount = 0;
						var slate = new Slate();
						slate.Set<float>("points", Rand.RangeInclusive(60000, 300000));
						Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Defs.QuestDefOf.LTS_Tenancy, new Slate());
						QuestUtility.SendLetterQuestAvailable(quest);
					}
					else
					{
						Messages.Message(Language.Translate.CourierTenancyNoticeFailFunds(courier),
							MessageTypeDefOf.NeutralEvent);
					}
				}

				Messages.Message(Language.Translate.CourierDelivered(courier), _noticeBoard,
					MessageTypeDefOf.NeutralEvent);

			}
			catch (Exception ex)
			{
				Log.Error($"LTS_Tenants Error - EmptyBoard: {ex.Message}\n{ex.StackTrace}");
			}
		}

		public void Payday(Contract contract)
		{
			_silver += contract._rent * contract.LengthDays;
		}

		public bool FindNoticeBoardInMap()
		{
			_noticeBoard = null;
			var noticeBoards = this.map.listerBuildings.allBuildingsColonist.OfType<NoticeBoard>().ToList();
			if (!noticeBoards.Any())
			{
				return false;
			}

			_noticeBoard = noticeBoards.FirstOrDefault();
			return true;

		}

		#endregion Methods

	}
}