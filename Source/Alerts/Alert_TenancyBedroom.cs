using System.Collections.Generic;
using Verse;
using RimWorld;
// ReSharper disable InconsistentNaming

namespace Tenants.Alerts
{
	public class Alert_TenancyBedroom : Alert
	{
		private readonly List<Pawn> _culpritsResult = new List<Pawn>();

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Culprits);
		}

		public Alert_TenancyBedroom()
		{
			this.defaultLabel = Language.Translate.TenancyRoomRequired.Translate();
			this.defaultExplanation = Language.Translate.TenancyRoomRequiredDesc.Translate();
		}

		private List<Pawn> Culprits
		{
			get
			{
				_culpritsResult.Clear();
				List<Map> maps = Find.Maps;
				if (!maps.Any())
				{
					return _culpritsResult;
				}

				for (int i = 0; i < maps.Count; i++)
				{
					Components.Tenants_MapComponent comp = maps[i].GetComponent<Components.Tenants_MapComponent>();
					if (!comp.ActiveContracts.Any())
					{
						continue;
					}

					for (int y = 0; y < comp.ActiveContracts.Count; y++)
					{
						Models.Contract contract = comp.ActiveContracts[i];
						if (contract._tenant != null && contract._tenant.HasExtraHomeFaction())
						{
							if (contract._singleRoomRequirement && !contract._tenant.royalty.HasPersonalBedroom())
							{
								_culpritsResult.Add(contract._tenant);
							}
						}
						else
						{
							comp.ActiveContracts.Remove(contract);
						}
					}
				}

				return _culpritsResult;
			}
		}

	}
}