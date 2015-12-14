using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MWF.Mobile.Core.Repositories
{
    public class ConfigRepository : Repository<MWFMobileConfig>, IConfigRepository
    {
        #region Construction

        public ConfigRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion

        /// <summary>
        /// Return the single config row
        /// </summary>
        public async Task<MWFMobileConfig> GetAsync()
        {
            var data = await this.GetAllAsync();
            Debug.Assert(data.Count() == 1, $"Expected one MWFMobileConfig record but found {data.Count()}");
            return data.First();
        }

    }
}
