using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories.Interfaces
{
    public interface IMobileDataRepository : IRepository<MobileData>
    {
        Task<bool> InstructionExists(Guid id);
        Task<IEnumerable<MobileData>> GetNonCompletedInstructions(Guid driverID);
        Task<IEnumerable<MobileData>> GetInProgressInstructions(Guid driverID);
        Task<IEnumerable<MobileData>> GetNotStartedInstructions(Guid driverID);
        Task<IEnumerable<MobileData>> GetNonCompletedMessages(Guid driverID);
        Task<IEnumerable<MobileData>> GetAllMessages(Guid driverID);

    }
}
