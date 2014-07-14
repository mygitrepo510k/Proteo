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
        ICustomerRepository CustomerRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IDriverRepository DriverRepository { get; }
        ISafetyProfileRepository SafetyProfileRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        IVerbProfileRepository VerbProfileRepository { get; }
    }
}
