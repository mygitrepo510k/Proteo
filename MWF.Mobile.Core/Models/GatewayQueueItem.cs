using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{
    
    public class GatewayQueueItem
    {
        public string JsonSerializedRequestContent { get; set; }
        public DateTime QueuedDateTime { get; set; }
    }

}
