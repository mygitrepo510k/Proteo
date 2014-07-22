using System;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IStartupInfoService
    {
        Driver LoggedInDriver { get; set; }
        SafetyCheckData CurrentSafetyCheckData { get; set; }
        Vehicle CurrentVehicle { get; set; }
        int Mileage { get; set; }
    }
}
