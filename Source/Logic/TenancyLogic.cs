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
                Log.Warning("Tenancy settings had no min/max days set, mod settings got reset!");
            }
            var contract = new Models.Contract {
                _tenant = tenant,
                _length = Rand.Range(Settings.Settings.Days.min, Settings.Settings.Days.max) * 60000,
                _startdate = Find.TickManager.TicksGame,
                _singleRoomRequirement = Rand.Bool
            };
            contract._endDate = Find.TickManager.TicksAbs + contract._length + 60000;
            contract._rent = (int)(contract._singleRoomRequirement ? Settings.Settings.Rent * 1.5f: Settings.Settings.Rent);
            return contract;
        }
    }
}
