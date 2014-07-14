using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Repositories
{

    public interface IGatewayQueueItemRepository : IRepository<GatewayQueueItem> 
    {

        IEnumerable<GatewayQueueItem> GetAllInQueueOrder();
    }

}
