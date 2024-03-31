using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Language;
using Tenants.Logic;
using UnityEngine;
using Verse;

namespace Tenants.Things
{
	[StaticConstructorOnStartup]
	public class NoticeBoard : Building
	{
		public bool _isActive = true;
		public bool _noticeUp;
		public int _silverAmount;
		public bool _femaleOnly, _maleOnly;
		public bool _singleRoom;
		public bool _violenceEnabled = true;
		public XenotypeDef _chosenXeno;
		public CustomXenotype _chosenCustomXeno;
		private List<XenotypeDef> _availableXenos = new List<XenotypeDef>();
		private List<CustomXenotype> _customXenotypes = new List<CustomXenotype>();
		private static readonly Texture2D _advertIcon = ContentFinder<Texture2D>.Get("Icons/AdvertIcon");
		public int _maxAge = 50;

		public Gender GetForcedGender()
		{
			if (_femaleOnly)
			{
				return Gender.Female;
			}

			return _maleOnly ? Gender.Male : Rand.Bool ? Gender.Female : Gender.Male;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			if (this.Faction != Faction.OfPlayer)
			{
				yield break;
			}

			//Place Advert
			var commandToggle = new Command_Toggle
			{
				defaultLabel = Translate.AdvertisementGizmo(),
				defaultDesc = Translate.AdvertisementGizmoDesc(),
				icon = _advertIcon,
				isActive = () => _isActive,
				toggleAction = delegate
				{
					_isActive = !_isActive;

					if (!_isActive)
					{
						DropSilver();
					}
				}
			};

			yield return commandToggle;

			//Room
			var roomToggle = new Command_Toggle
			{
				defaultLabel = Translate.AdvertisementSingleRoomGizmo(),
				defaultDesc = Translate.AdvertisementSingleRoomGizmoDesc(),
				icon = ThingDefOf.Bed.uiIcon,
				isActive = () => _singleRoom,
				toggleAction = delegate
				{
					_singleRoom = !_singleRoom;
					DropSilver();
				}
			};

			yield return roomToggle;

			//Room
			var violenceToggle = new Command_Toggle
			{
				defaultLabel = Translate.AdvertisementFightableGizmo(),
				defaultDesc = Translate.AdvertisementFightableGizmoDesc(),
				icon = TexCommand.Draft,
				isActive = () => _violenceEnabled,
				toggleAction = delegate
				{
					_violenceEnabled = !_violenceEnabled;
					DropSilver();
				}
			};

			yield return violenceToggle;

			//Gender
			var genderToggle = new Command_Action
			{
				defaultLabel = _femaleOnly
					? Translate.AdvertisementFemaleGizmo()
					: _maleOnly
						? Translate.AdvertisementMaleGizmo()
						: Translate.AdvertisementGenderGizmo(),
				icon = _femaleOnly
					? LTS_Systems.GUI.Icons.Female
					: _maleOnly
						? LTS_Systems.GUI.Icons.Male
						: LTS_Systems.GUI.Icons.Info,
				action = delegate
				{
					if (_femaleOnly)
					{
						_maleOnly = true;
						_femaleOnly = false;
					}
					else if (_maleOnly)
					{
						_maleOnly = false;
						_femaleOnly = false;
					}
					else
					{
						_femaleOnly = true;
						_maleOnly = false;
					}

					DropSilver();
				}
			};

			yield return genderToggle;

			//Age
			var ageToggle = new Command_Action()
			{
				defaultLabel =
					_maxAge > 40 ? Translate.AdvertisementAnyAge() : Translate.AdvertisementAge(_maxAge),
				icon = LTS_Systems.GUI.Icons.Info,
				action = delegate
				{
					if (_maxAge <= 20)
					{
						_maxAge = 50;
					}
					else
					{
						_maxAge -= 10;
					}

					DropSilver();
				}
			};

			yield return ageToggle;

			//Xeno
			if (!ModLister.BiotechInstalled)
			{
				yield break;
			}

			var xenoToggle = new Command_Action
			{
				defaultLabel = _chosenXeno != null ? _chosenXeno.label
					: _chosenCustomXeno != null ? _chosenCustomXeno.name
					: Translate.AdvertisementXenosAnyGizmo(),
				icon = _chosenXeno != null ? _chosenXeno.Icon
					: _chosenCustomXeno != null ? _chosenCustomXeno.iconDef.Icon
					: LTS_Systems.GUI.Icons.Info,
				action = delegate
				{
					Find.WindowStack.Add(new FloatMenu(GenerateXenoMenu().ToList()));
				}
			};

			yield return xenoToggle;
		}

		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine(Translate.AdvertisementCost() + " " + AdvertisementCost());
			stringBuilder.AppendLine(Translate.ExpectedRent(CalculateRent()));
			if (_noticeUp)
			{
				stringBuilder.AppendLine(Translate.AdvertisementPlaced());
			}

			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _noticeUp, "NoticeUp");
			Scribe_Values.Look(ref _isActive, "IsActive", true);
			Scribe_Values.Look(ref _femaleOnly, "FemaleOnly");
			Scribe_Values.Look(ref _maleOnly, "MaleOnly");
			Scribe_Values.Look(ref _maxAge, "MaxAge");
			Scribe_Values.Look(ref _singleRoom, "SingleRoom");
			Scribe_Values.Look(ref _violenceEnabled, "ViolenceEnabled");
			Scribe_Defs.Look(ref _chosenXeno, "ChosenXeno");
			Scribe_Deep.Look(ref _chosenCustomXeno, "ChosenCustomXeno");
			Scribe_Values.Look(ref _silverAmount, "SilverAmount");
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			DropSilver();
			base.DeSpawn(mode);
		}

		private void DropSilver()
		{
			_noticeUp = false;
			if (_silverAmount < 1)
			{
				return;
			}

			DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, this.Position, _silverAmount);
			_silverAmount = 0;
		}

		public int AdvertisementCost()
		{
			return Mathf.Min(500, (int)(Settings.Settings.NoticeCourierCost * IncrementTimes()));
		}

		public float IncrementTimes()
		{
			float amount = 1;
			amount = _femaleOnly ? amount * 1.5f : amount;
			amount = _maleOnly ? amount * 1.5f : amount;
			amount = _chosenCustomXeno != null ? amount * 2f : amount;
			amount = _chosenXeno != null ? amount * 2f : amount;
			amount *= TenancyLogic.AgeValueCalculator(_maxAge);
			return amount;
		}

		public int CalculateRent()
		{
			int amount = Settings.Settings.Rent;
			amount = (int)(_singleRoom ? amount + (Settings.Settings.Rent * 0.5f) : amount);
			amount = (int)(!_violenceEnabled ? amount + (Settings.Settings.Rent * 0.5f) : amount);
			amount = (int)(amount / IncrementTimes());
			return amount;
		}

		private IEnumerable<FloatMenuOption> GenerateXenoMenu()
		{
			yield return new FloatMenuOption(Translate.AdvertisementXenosAnyGizmo(), delegate
			{
				_chosenXeno = null;
				_chosenCustomXeno = null;
				DropSilver();
			});

			_availableXenos = DefDatabase<XenotypeDef>.AllDefsListForReading;
			using (List<XenotypeDef>.Enumerator enumerator = _availableXenos.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XenotypeDef xenoDef = enumerator.Current;

					if (xenoDef == null)
					{
						continue;
					}

					if (xenoDef == XenotypeDefOf.Sanguophage)
					{
						continue;
					}

					if (xenoDef == _chosenXeno)
					{
						continue;
					}

					yield return new FloatMenuOption(xenoDef.LabelCap, delegate
					{
						_chosenXeno = xenoDef;
						_chosenCustomXeno = null;
						DropSilver();
					});
				}
			}

			_customXenotypes = Current.Game.customXenotypeDatabase.customXenotypes;
			using (List<CustomXenotype>.Enumerator enumerator = _customXenotypes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CustomXenotype xenoDef = enumerator.Current;

					if (xenoDef == null)
					{
						continue;
					}

					if (xenoDef == _chosenCustomXeno)
					{
						continue;
					}

					yield return new FloatMenuOption(xenoDef.name, delegate
					{
						_chosenCustomXeno = xenoDef;
						_chosenXeno = null;
						DropSilver();
					});
				}
			}
		}
	}
}