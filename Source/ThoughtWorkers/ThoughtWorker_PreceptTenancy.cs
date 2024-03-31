using RimWorld;
using Verse;

namespace Tenants.ThoughtWorkers
{
	public class ThoughtWorker_PreceptTenancy : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return ThoughtState.Inactive;
			}

			if (p.IsQuestLodger())
			{
				return ThoughtState.Inactive;
			}

			if (!p.Map.IsPlayerHome)
			{
				return ThoughtState.Inactive;
			}

			return ThoughtState.ActiveAtStage(
				p.Map?.mapPawns?.FreeColonistsSpawned?.Count(x => x.HasExtraHomeFaction()) ?? 0);
		}
	}
}