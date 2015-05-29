using System;
using MWF.Mobile.Core.Models;
using System.Collections.Generic;

namespace MWF.Mobile.Core.Services
{
    public interface ISafetyCheckService
    {
        void CommitSafetyCheckData(bool trailerOnly = false);
        SafetyCheckData CurrentTrailerSafetyCheckData { get; set; }
        SafetyCheckData CurrentVehicleSafetyCheckData { get; set; }
        IEnumerable<MWF.Mobile.Core.Models.SafetyCheckData> GetCurrentSafetyCheckData();
        IEnumerable<MWF.Mobile.Core.Models.SafetyCheckData> GetSafetyCheckData(SafetyCheckData vehicleSafetyCheckData, SafetyCheckData trailerSafetyCheckData);
    }
}
