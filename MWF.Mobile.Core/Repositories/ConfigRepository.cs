﻿using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories
{
    public class ConfigRepository : Repository<MWFMobileConfig>, IConfigRepository
    {
        #region Construction

        public ConfigRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion
    }
}
