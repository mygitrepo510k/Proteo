using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories
{
    public class Repositories : IRepositories
    {

        public Repositories(IApplicationProfileRepository applicationRepository, ICustomerRepository customerRepository, IDeviceRepository deviceRepository,
                            IDriverRepository driverRepository, ISafetyProfileRepository safetyProfileRepository, IVehicleRepository vehicleRepository, IVerbProfileRepository verbProfileRepository, 
                            IGatewayQueueItemRepository gatewayQueueItemRepository, ITrailerRepository trailerRepository)

        {

            ApplicationRepository = applicationRepository;
            CustomerRepository = customerRepository;
            DeviceRepository = deviceRepository;
            DriverRepository = driverRepository;
            GatewayQueueItemRepository = gatewayQueueItemRepository;
            SafetyProfileRepository = safetyProfileRepository;
            TrailerRepository = trailerRepository;
            VehicleRepository = vehicleRepository;
            VerbProfileRepository = verbProfileRepository;
            GatewayQueueItemRepository = gatewayQueueItemRepository;
            TrailerRepository = trailerRepository;
        }

        public IApplicationProfileRepository ApplicationRepository
        {
            get;
            private set;
        }

        public ICustomerRepository CustomerRepository
        {
            get;
            private set;
        }

        public IDeviceRepository DeviceRepository
        {
            get;
            private set;
        }

        public IDriverRepository DriverRepository
        {
            get;
            private set;
        }

        public IGatewayQueueItemRepository GatewayQueueItemRepository
        {
            get;
            private set;
        }

        public ISafetyProfileRepository SafetyProfileRepository
        {
            get;
            private set;
        }

        public ITrailerRepository TrailerRepository
        {
            get;
            private set;
        }

        public IVehicleRepository VehicleRepository
        {
            get;
            private set;
        }

        public IVerbProfileRepository VerbProfileRepository
        {
            get;
            private set;
        }

    }
}
