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
        bool InstructionExists(Guid id);
        IEnumerable<MobileData> GetInProgressInstructions(Guid driverID);
        IEnumerable<MobileData> GetNotStartedInstructions(Guid driverID);
        IEnumerable<MobileData> GetNoneCompletedMessages(Guid driverID);
        IEnumerable<MobileData> GetAllMessages(Guid driverID);

    }
}
