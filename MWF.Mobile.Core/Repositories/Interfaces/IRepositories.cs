﻿using MWF.Mobile.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories
{
    public interface IRepositories
    {
        IApplicationProfileRepository ApplicationRepository { get; }
        ICurrentDriverRepository CurrentDriverRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IDriverRepository DriverRepository { get; }
        IGatewayQueueItemRepository GatewayQueueItemRepository { get; }
        ILatestSafetyCheckRepository LatestSafetyCheckRepository { get; }
        ISafetyProfileRepository SafetyProfileRepository { get; }
        ITrailerRepository TrailerRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        IVerbProfileRepository VerbProfileRepository { get; }
        IConfigRepository ConfigRepository { get; }
    }
}
