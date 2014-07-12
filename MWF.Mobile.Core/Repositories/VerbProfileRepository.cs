using System;
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

    public class VerbProfileRepository : Repository<VerbProfile>, IVerbProfileRepository
    {

        #region Construction

        public VerbProfileRepository(IDataService dataService)
            : base(dataService)
        { }


        #endregion      

    }

}
