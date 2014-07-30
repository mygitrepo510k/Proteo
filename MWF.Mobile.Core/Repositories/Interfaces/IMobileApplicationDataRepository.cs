using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Repositories.Interfaces
{
    public interface IMobileApplicationDataRepository : IRepository<MobileApplicationData>
    {
        IEnumerable<MobileApplicationData> GetInProgressInstructions();
        IEnumerable<MobileApplicationData> GetNotStartedInstructions();
    }
}
