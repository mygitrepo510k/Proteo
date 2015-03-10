﻿using MWF.Mobile.Core.Models.GatewayServiceRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public interface IGatewayService
    {
        Task<Models.ApplicationProfile> GetApplicationProfile();
        Task<Models.Device> GetDevice(string customerID);
        Task<IEnumerable<Models.Driver>> GetDrivers();
        Task<IEnumerable<Models.SafetyProfile>> GetSafetyProfiles();
        Task<IEnumerable<Models.Vehicle>> GetVehicles(string vehicleViewTitle);
        Task<IEnumerable<Models.VehicleView>> GetVehicleViews();
        Task<Models.VerbProfile> GetVerbProfile(string verbProfileTitle);
        Task<IEnumerable<Models.Instruction.MobileData>> GetDriverInstructions(string vehicleRegistration,
                                                                               Guid driverTitle, 
                                                                               DateTime startDate,
                                                                               DateTime endDate);
        Task<Models.MWFMobileConfig> GetConfig();
        Task<bool> CreateDevice();
        Task<HttpResult> PostLogMessageAsync(DeviceLogMessage log);
    }

}
