using System;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IStartupInfoService
    {
        Driver LoggedInDriver { get; set; }
        Vehicle CurrentVehicle { get; set; }
        Trailer CurrentTrailer { get; set; }
        int Mileage { get; set; }
        SafetyCheckData CurrentSafetyCheckData { get; set; } // Note: the two properties below should be used... leaving this in for now in order not to break the build
        SafetyCheckData VehicleSafetyCheckData { get; set; }
        SafetyCheckData TrailerSafetyCheckData { get; set; }
    }
}
