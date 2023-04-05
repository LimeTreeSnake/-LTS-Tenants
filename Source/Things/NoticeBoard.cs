using RimWorld;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Tenants.Components;
using UnityEngine;
using Verse;

namespace Tenants.Things
{
	[StaticConstructorOnStartup]
	public class NoticeBoard : Building
	{
		public bool _isActive = true;
		public bool _noticeUp;
		private static readonly Texture2D _advertIcon = ContentFinder<Texture2D>.Get("Icons/AdvertIcon", true);

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

			var commandToggle = new Command_Toggle
			{
				defaultLabel = Language.Translate.AdvertisementGizmo(),
				defaultDesc = Language.Translate.AdvertisementGizmo(),
				hotKey = KeyBindingDefOf.Misc3,
				icon = _advertIcon,
				isActive = () => _isActive,
				toggleAction = delegate
				{
					_isActive = !_isActive;
				}
			};
			
			yield return commandToggle;
		}

		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			return _noticeUp ? Language.Translate.AdvertisementPlaced : stringBuilder.ToString();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _noticeUp, "NoticeUp");
			Scribe_Values.Look(ref _isActive, "IsActive", true);
		}
	}
}