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

        public async Task<bool> InstructionExistsAsync(Guid id)
        {
            return (await this.GetByIDAsync(id) != null);

        }

        public async Task<IEnumerable<MobileData>> GetNonCompletedInstructionsAsync(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            parentItems = await connection
               .Table<MobileData>()
               .Where(m =>
                      m.DriverId == driverID &&
                     (m.ProgressState != Enums.InstructionProgress.Complete)).ToListAsync();

                await PopulateChildrenRecursiveAsync(parentItems, connection);


            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public async Task<IEnumerable<MobileData>> GetInProgressInstructionsAsync(Guid driverID)
        {

            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            

                parentItems = await connection
                   .Table<MobileData>()
                   .Where(m =>
                          m.DriverId == driverID &&
                         (m.ProgressState == Enums.InstructionProgress.Driving || m.ProgressState == Enums.InstructionProgress.OnSite)).ToListAsync();

                await PopulateChildrenRecursiveAsync(parentItems, connection);

            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }

        public async Task<IEnumerable<MobileData>> GetNotStartedInstructionsAsync(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
                parentItems = await  connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID &&
                           (m.ProgressState == Enums.InstructionProgress.NotStarted)).ToListAsync();

                await PopulateChildrenRecursiveAsync(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type != Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }




        public async Task<IEnumerable<MobileData>> GetNonCompletedMessagesAsync(Guid driverID)
        {
            List<MobileData> parentItems;

            var data = await this.GetAllMessagesAsync(driverID);
            parentItems =  data.Where(m => m.ProgressState != Enums.InstructionProgress.Complete).ToList();

            return parentItems;
        }


        public async Task<IEnumerable<MobileData>> GetAllMessagesAsync(Guid driverID)
        {
            List<MobileData> parentItems;

            var connection = _dataService.GetAsyncDBConnection();
            
                parentItems = await connection
                    .Table<MobileData>()
                    .Where(m =>
                           m.DriverId == driverID).ToListAsync();

                await PopulateChildrenRecursiveAsync(parentItems, connection);
            
            parentItems = parentItems.Where(pi =>
                pi.Order.Type == Enums.InstructionType.OrderMessage).ToList();

            return parentItems;
        }
    }
}
