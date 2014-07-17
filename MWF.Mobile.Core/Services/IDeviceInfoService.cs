using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    
    public interface IDeviceInfoService
    {
        string DeviceIdentifier { get; }
        string GatewayPassword { get; }
        string MobileApplication { get; }
    }

}
