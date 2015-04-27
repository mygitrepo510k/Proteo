﻿using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public interface IStartupService
    {
        Driver LoggedInDriver { get; set; }
        SafetyCheckData CurrentVehicleSafetyCheckData { get; set; }
        SafetyCheckData CurrentTrailerSafetyCheckData { get; set; }
        Vehicle CurrentVehicle { get; set; }
        Trailer CurrentTrailer { get; set; }
        int Mileage { get; set; }

        IEnumerable<SafetyCheckData> GetCurrentSafetyCheckData();
        IEnumerable<Models.SafetyCheckData> GetSafetyCheckData(Models.SafetyCheckData vehicleSafetyCheckData, Models.SafetyCheckData trailerSafetyCheckData);
        void StartGatewayQueueTimer();
        void CommitSafetyCheckData();
        void DriverLogIn();
    }
}
