using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    /// <summary>
    /// This is just a stub for now to provide info we need for the gateway service calls - this service may be implemented later or may be removed and replaced with something more suitable.
    /// </summary>
    public class DeviceInfoService
        : IDeviceInfoService
    {

        public string DeviceIdentifier
        {
            get { return "021PROTEO0000001"; }
        }

        public string GatewayPassword
        {
            get { return "fleetwoodmobile"; }
        }

    }

}
