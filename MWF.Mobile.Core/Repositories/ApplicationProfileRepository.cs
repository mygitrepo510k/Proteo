﻿using System;
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

    public class ApplicationProfileRepository : Repository<ApplicationProfile>, IApplicationProfileRepository
    {

        #region Construction

        public ApplicationProfileRepository(IDataService dataService)
            : base(dataService)
        { }


        #endregion

    }

}
