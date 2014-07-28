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

    public class LatestSafetyCheckRepository : Repository<LatestSafetyCheck>, ILatestSafetyCheckRepository
    {

        #region Construction

        public LatestSafetyCheckRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion

        public LatestSafetyCheck GetForDriver(Guid driverID)
        {
            return this.GetAll().FirstOrDefault(lsc => lsc.DriverID == driverID);
        }

        public void SetForDriver(LatestSafetyCheck latestSafetyCheck)
        {
            if (latestSafetyCheck.VehicleSafetyCheck == null && latestSafetyCheck.TrailerSafetyCheck == null)
                return;

            var latestSafetyCheckForDriver = this.GetForDriver(latestSafetyCheck.DriverID);

            // Delete any existing latest safety check for the driver before adding a new one.
            // Currently this deletes both vehicle and trailer safety checks and it is possible that one of these will be null in the latestSafetyCheck object - if this is the case should we actually be retaining the older safety check?
            if (latestSafetyCheckForDriver != null)
                this.Delete(latestSafetyCheckForDriver);

            this.Insert(latestSafetyCheck);
        }

    }

}
