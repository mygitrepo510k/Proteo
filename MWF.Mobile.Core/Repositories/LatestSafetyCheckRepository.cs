using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
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

        public async Task<LatestSafetyCheck> GetForDriver(Guid driverID)
        {
            var a = await this.GetAllAsync();
            return a.FirstOrDefault(lsc => lsc.DriverID == driverID);
        }

        public async Task SetForDriver(LatestSafetyCheck latestSafetyCheck)
        {
            var latestSafetyCheckForDriver = await this.GetForDriver(latestSafetyCheck.DriverID);

            // Delete any existing latest safety check for the driver before adding a new one.
            // Currently this deletes both vehicle and trailer safety checks and it is possible that one of these will be null in the latestSafetyCheck object - if this is the case should we actually be retaining the older safety check?
            if (latestSafetyCheckForDriver != null)
                await this.DeleteAsync(latestSafetyCheckForDriver);

            await this.InsertAsync(latestSafetyCheck);
        }

    }

}
