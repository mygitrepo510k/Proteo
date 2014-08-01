using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public class QueueAction<TData>
        where TData: class
    {
        public string Command { get; set; }
        public TData Data { get; set; }
        public Models.GatewayServiceRequest.Parameter[] Parameters { get; set; }
    }

    public interface IGatewayQueuedService
    {
        void StartQueueTimer();
        void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null);
        void AddToQueue<TData>(string command, TData data, Models.GatewayServiceRequest.Parameter[] parameters = null) where TData : class;
        void AddToQueue<TData>(IEnumerable<Models.GatewayServiceRequest.Action<TData>> actions) where TData : class;
    }

}
