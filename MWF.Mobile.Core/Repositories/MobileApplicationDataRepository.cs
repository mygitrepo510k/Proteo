using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories
{
    public class MobileApplicationDataRepository : Repository<MobileApplicationData>, IMobileApplicationDataRepository
    {
        #region Construction
        public MobileApplicationDataRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion

        public IEnumerable<MobileApplicationData> GetInProgressInstructions()
        {
            return _connection
                .Table<MobileApplicationData>()
                .Where(m => m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite);
        }

        public IEnumerable<MobileApplicationData> GetNotStartedInstructions()
        {
            return _connection
                .Table<MobileApplicationData>()
                .Where(m => m.ProgressState == Enums.InstructionProgress.NotStarted);
        }
    }
}
