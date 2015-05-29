using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IInfoService
    {
        Driver LoggedInDriver { get; set; }
        Vehicle CurrentVehicle { get; set; }
        Trailer CurrentTrailer { get; set; }
        int Mileage { get; set; }
    }
}
