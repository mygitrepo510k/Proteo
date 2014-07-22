using System;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IStartupInfoService
    {
        Driver LoggedInDriver { get; set; }
        Vehicle Vehicle { get; set; }
        Trailer Trailer { get; set; }
        SafetyCheckData VehicleSafetyCheckData { get; set; }
        SafetyCheckData TrailerSafetyCheckData { get; set; }
    }
}
