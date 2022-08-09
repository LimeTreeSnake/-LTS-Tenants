using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Tenants.Logic {
    public static class TenancyLogic {
       
        public static Models.Contract GenerateBasicTenancyContract(Pawn tenant) {
            if (Settings.Settings.Days.min == 0) {
                Settings.Settings.Reset();
            }
            Models.Contract contract = new Models.Contract {
                tenant = tenant,
                length = Rand.Range(Settings.Settings.Days.min, Settings.Settings.Days.max) * 60000,
                startdate = Find.TickManager.TicksGame
            };
            contract.endDate = Find.TickManager.TicksAbs + contract.length + 60000;
            contract.rent = Settings.Settings.Rent;
            return contract;
        }
    }
}
