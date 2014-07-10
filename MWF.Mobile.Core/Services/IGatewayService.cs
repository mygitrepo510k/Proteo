using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public interface IGatewayService
    {
        Task<Models.Device> GetDevice();
        Task<IEnumerable<Models.Driver>> GetDrivers();
    }

}
