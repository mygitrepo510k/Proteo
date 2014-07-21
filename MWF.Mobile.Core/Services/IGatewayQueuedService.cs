using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public interface IGatewayQueuedService
    {
        void StartQueueTimer();
        void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null);
        void AddToQueueAndSubmit(string command, Models.GatewayServiceRequest.Parameter[] parameters = null);
    }

}
