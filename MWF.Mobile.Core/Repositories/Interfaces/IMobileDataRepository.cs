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
        IEnumerable<MobileData> GetInProgressInstructions();
        IEnumerable<MobileData> GetNotStartedInstructions();
    }
}
