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
    public class MobileDataRepository : Repository<MobileData>, IMobileDataRepository
    {
        #region Construction
        public MobileDataRepository(IDataService dataService)
            : base(dataService)
        { }

        #endregion

        public bool InstructionExists(Guid id)
        {
            var instruction = _connection.Table<MobileData>().Where(m => m.ID == id);
            return (instruction != null);
        }

        public IEnumerable<MobileData> GetInProgressInstructions()
        {
            return _connection
                .Table<MobileData>()
                .Where(m => m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite);
        }

        public IEnumerable<MobileData> GetNotStartedInstructions()
        {
            return _connection
                .Table<MobileData>()
                .Where(m => m.ProgressState == Enums.InstructionProgress.NotStarted);
        }
    }
}
