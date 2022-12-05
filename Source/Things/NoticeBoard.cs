using RimWorld;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Tenants.Components;
using UnityEngine;
using Verse;

namespace Tenants.Things {
    [StaticConstructorOnStartup]
    public class NoticeBoard : Building {
        public bool isActive = true;
        public bool noticeUp = false;
        private static readonly Texture2D AdvertIcon = ContentFinder<Texture2D>.Get("Icons/AdvertIcon", true);
        public override IEnumerable<Gizmo> GetGizmos() {
            foreach (Gizmo gizmo in base.GetGizmos()) {
                yield return gizmo;
            }
            if (base.Faction == Faction.OfPlayer) {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = Language.Translate.AdvertisementGizmo();
                command_Toggle.defaultDesc = Language.Translate.AdvertisementGizmo();
                command_Toggle.hotKey = KeyBindingDefOf.Misc3;
                command_Toggle.icon = AdvertIcon;
                command_Toggle.isActive = () => isActive;
                command_Toggle.toggleAction = delegate
                {
                    isActive = !isActive;
                };
                yield return command_Toggle;
            }
        }
        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (noticeUp)
                return Language.Translate.AdvertisementPlaced;
            return stringBuilder.ToString();
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref noticeUp, "NoticeUp", false);
            Scribe_Values.Look(ref isActive, "IsActive", true);
        }
    }
}
