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

    public class SafetyProfileRepository : RepositoryWithChidren<SafetyProfile, SafetyCheckFaultType>,
                                           ISafetyProfileRepository
    {

        #region Construction

        public SafetyProfileRepository(IDataService dataService)
            : base(dataService)
        { }


        #endregion


        protected override void PopulateChildren(IEnumerable<SafetyProfile> parents)
        {
            foreach (var parent in parents)
            {
                parent.Children = _connection.Table<SafetyCheckFaultType>().Where(e => e.SafetyProfileID == parent.ID).ToList();
            }

        }

    }

}
