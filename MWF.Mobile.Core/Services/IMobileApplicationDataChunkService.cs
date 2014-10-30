using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IMobileApplicationDataChunkService
    {
        MobileData CurrentMobileData { get; set; }
        void Commit();
    }
}
