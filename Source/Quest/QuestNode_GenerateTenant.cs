using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Linq;
using Verse;

namespace Tenants.QuestNodes
{
	public class QuestNode_GenerateTenant : QuestNode
	{
		// ReSharper disable MemberCanBePrivate.Global
		// ReSharper disable InconsistentNaming
		[NoTranslate] public SlateRef<string> tenant;
		[NoTranslate] public SlateRef<string> race;
		[NoTranslate] public SlateRef<string> gender;
		[NoTranslate] public SlateRef<string> genes;
		[NoTranslate] public SlateRef<string> addToList;
		[NoTranslate] public SlateRef<string> tenantFaction;
		[NoTranslate] public SlateRef<bool> rejected;

		public SlateRef<Map> map;
		// ReSharper restore MemberCanBePrivate.Global
		// ReSharper restore InconsistentNaming

		protected override void RunInt()
		{
			try
			{
				Slate slate = QuestGen.slate;
				if (map.GetValue(slate) == null)
				{
					Log.Error("No map found for the quest slate");
					return;
				}

				map.TryGetValue(slate, out Map mapStuff);
				Components.Tenants_MapComponent comp = mapStuff.GetComponent<Components.Tenants_MapComponent>();
				if (comp == null)
				{
					mapStuff.components.Add(new Components.Tenants_MapComponent(mapStuff));
				}

				Pawn newTenant = comp?.GetTenant();
				if (newTenant == null)
				{
					return;
				}

				if (tenant.GetValue(slate) != null)
				{
					QuestGen.slate.Set(tenant.GetValue(slate), newTenant);
				}

				if (race.GetValue(slate) != null)
				{
					QuestGen.slate.Set(race.GetValue(slate), newTenant.def.label);
				}

				if (gender.GetValue(slate) != null)
				{
					QuestGen.slate.Set(gender.GetValue(slate), newTenant.gender.GetLabel());
				}

				if (genes.GetValue(slate) != null)
				{
					QuestGen.slate.Set(genes.GetValue(slate),
						ModLister.BiotechInstalled ? newTenant.genes.XenotypeLabel : "");
				}

				if (addToList.GetValue(slate) != null)
				{
					QuestGenUtility.AddToOrMakeList(QuestGen.slate, addToList.GetValue(slate), newTenant);
				}

				if (tenantFaction.GetValue(slate) != null)
				{
					QuestGen.slate.Set(tenantFaction.GetValue(slate), newTenant.Faction);
				}

				QuestGen.slate.Set("rejected", comp.TenantKills > 0);
				if (!Settings.Settings.KillPenalty)
				{
					comp.TenantKills = 0;
				}
			}
			catch (Exception ex)
			{
				Log.Message("Error at QuestNode_GenerateTenant RunInt: " + ex.Message);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			return slate.Exists("map");
		}
	}
}