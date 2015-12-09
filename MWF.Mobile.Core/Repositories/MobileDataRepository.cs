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

        public async Task<bool> InstructionExists(Guid id)
        {
            return (await this.GetByIDAsync(id) != null);

        }

        public async Task<IEnumerable<MobileData>> GetNonCompletedInstructions(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            parentItems = await connection
               .Table<MobileData>()
               .Where(m =>
                      m.DriverId == driverID &&
                     (m.ProgressState != Enums.InstructionProgress.Complete)).ToListAsync();

                await PopulateChildrenRecursive(parentItems, connection);


            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public async Task<IEnumerable<MobileData>> GetInProgressInstructions(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            

                parentItems = await connection
                   .Table<MobileData>()
                   .Where(m =>
                          m.DriverId == driverID &&
                         (m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite)).ToListAsync();

                await PopulateChildrenRecursive(parentItems, connection);

            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public async Task<IEnumerable<MobileData>> GetNotStartedInstructions(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
                parentItems = await  connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID &&
                           (m.ProgressState == Enums.InstructionProgress.NotStarted)).ToListAsync();

                await PopulateChildrenRecursive(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }




        public async Task<IEnumerable<MobileData>> GetNonCompletedMessages(Guid driverID)
        {
            List<MobileData> parentItems;

            var data = await this.GetAllMessages(driverID);
            parentItems =  data.Where(m => m.ProgressState != Enums.InstructionProgress.Complete).ToList();

            return parentItems;
        }


        public async Task<IEnumerable<MobileData>> GetAllMessages(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            
                parentItems = await connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID).ToListAsync();

                await PopulateChildrenRecursive(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type == Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }
    }
}
