using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories
{

    public interface ILatestSafetyCheckRepository : IRepository<LatestSafetyCheck> 
    {
        Task<LatestSafetyCheck> GetForDriver(Guid driverID);
        Task SetForDriver(LatestSafetyCheck latestSafetyCheck);
    }

}
