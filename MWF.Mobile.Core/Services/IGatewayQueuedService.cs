using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public interface IGatewayQueuedService
    {
        void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null);
        Task<bool> AddToQueueAndSubmitAsync(string command, Models.GatewayServiceRequest.Parameter[] parameters = null);
    }

}
