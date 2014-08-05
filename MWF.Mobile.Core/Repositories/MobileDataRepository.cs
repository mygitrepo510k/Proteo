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
            var parentItems = _connection
               .Table<MobileData>()
               .Where(m => m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite).ToList();

            PopulateChildrenRecursive(parentItems);

            return parentItems;
        }

        public IEnumerable<MobileData> GetNotStartedInstructions()
        {
            var parentItems = _connection
                .Table<MobileData>()
                .Where(m => m.ProgressState == Enums.InstructionProgress.NotStarted).ToList();

            PopulateChildrenRecursive(parentItems);

            return parentItems;
        }

        /**
         * TODO: Implement GetMessageWithPointInstructions
        public IEnumerable<MobileData> GetMessageWithPointInstructions()
        {
            var parentItems = _connection
                .Table<MobileData>()
                .Where(m => m.Order.Type == Enums.InstructionType.MessageWithPoint).ToList();

            PopulateChildrenRecursive(parentItems);

            return parentItems;
        }
         */ 
    }
}
