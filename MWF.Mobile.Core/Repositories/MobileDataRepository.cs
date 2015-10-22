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
        public MobileDataRepository(IDataService dataService, ILoggingService loggingService)
            : base(dataService, loggingService)
        { }

        #endregion

        public bool InstructionExists(Guid id)
        {
            return (this.GetByID(id) != null);

        }

        public IEnumerable<MobileData> GetNonCompletedInstructions(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetDBConnection();
                parentItems = connection
                   .Table<MobileData>()
                   .Where(m =>
                          m.DriverId == driverID &&
                         (m.ProgressState != Enums.InstructionProgress.Complete)).ToList();

                PopulateChildrenRecursive(parentItems, connection);


            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public IEnumerable<MobileData> GetInProgressInstructions(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetDBConnection();
            

                parentItems = connection
                   .Table<MobileData>()
                   .Where(m =>
                          m.DriverId == driverID &&
                         (m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite)).ToList();

                PopulateChildrenRecursive(parentItems, connection);

            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public IEnumerable<MobileData> GetNotStartedInstructions(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetDBConnection();
                parentItems = connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID &&
                           (m.ProgressState == Enums.InstructionProgress.NotStarted)).ToList();

                PopulateChildrenRecursive(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }




        public IEnumerable<MobileData> GetNonCompletedMessages(Guid driverID)
        {
            List<MobileData> parentItems;

            parentItems = this.GetAllMessages(driverID).Where(m => m.ProgressState != Enums.InstructionProgress.Complete).ToList();

            return parentItems;
        }


        public IEnumerable<MobileData> GetAllMessages(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetDBConnection();
            
                parentItems = connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID).ToList();

                PopulateChildrenRecursive(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type == Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }
    }
}
