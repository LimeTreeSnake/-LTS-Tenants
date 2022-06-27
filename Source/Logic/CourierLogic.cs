using Verse;
using RimWorld;
using Tenants.Components;
using System.Collections.Generic;
using System;
using Verse.AI.Group;
using System.Linq;

namespace Tenants.Logic {
    public static class CourierLogic {


        public static bool CourierEvent(IncidentParms parms, Components.Tenants_MapComponent component) {
            try {
                Map map = (Map)parms.target;
                RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 loc, map, CellFinder.EdgeRoadChance_Neutral, false, null);
                Pawn courier = component.GetCourier();
                if (courier != null) {
                    //if (!Settings.Settings.KillPenalty)
                        component.CourierKills = 0;
                    if (component.CourierKills > 0) {
                        Find.LetterStack.ReceiveLetter(Language.Translate.CourierDenied, Language.Translate.CourierDeniedMessage(courier), LetterDefOf.NegativeEvent);
                        Log.Message(component.CourierKills.ToString());
                        component.CourierKills--;
                    }
                    else {
                        GenSpawn.Spawn(courier, loc, map);
                        courier.relations.everSeenByPlayer = true;
                        Find.LetterStack.ReceiveLetter(Language.Translate.CourierArrival, Language.Translate.CourierArrivalMessage(courier), LetterDefOf.PositiveEvent, courier);
                        LordMaker.MakeNewLord(courier.Faction, new Lords.Courier_LordJob(), map, new List<Pawn> { courier });
                    }
                }
                else {
                    Log.Message("Found no Courier :(  " + component);
                    map.components.Remove(component);
                    foreach (Type current in typeof(MapComponent).AllSubclassesNonAbstract()) {
                        if (map.GetComponent(current) == null) {
                            try {
                                MapComponent co = (MapComponent)Activator.CreateInstance(current, new object[]
                                {
                            map
                                });
                                map.components.Add(co);
                            }
                            catch (Exception ex) {
                                Log.Error(string.Concat(new object[]
                                {
                            "Could not instantiate a MapComponent of type ",
                            current,
                            ": ",
                            ex
                                }));
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex) {
                Log.Message("Error at CourierEvent method: " + ex.Message);
                return false;
            }
        }
    }
}
