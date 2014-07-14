﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.Repositories
{

    public class GatewayQueueItemRepository : Repository<GatewayQueueItem>, IGatewayQueueItemRepository
    {

        #region Construction

        public GatewayQueueItemRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion

        public IEnumerable<GatewayQueueItem> GetAllInQueueOrder()
        {
            return this.GetAll().OrderBy(gqi => gqi.QueuedDateTime);
        }

    }

}
