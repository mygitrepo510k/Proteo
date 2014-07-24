using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Repositories
{

    public interface ILatestSafetyCheckRepository : IRepository<LatestSafetyCheck> 
    {
        LatestSafetyCheck GetForDriver(Guid driverID);
        void SetForDriver(LatestSafetyCheck latestSafetyCheck);
    }

}
