using System;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IStartupInfoService
    {
        Driver LoggedInDriver { get; set; }
        SafetyCheckData CurrentVehicleSafetyCheckData { get; set; }
        SafetyCheckData CurrentTrailerSafetyCheckData { get; set; }
        Vehicle CurrentVehicle { get; set; }
        Trailer CurrentTrailer { get; set; }
        int Mileage { get; set; }

    }
}
