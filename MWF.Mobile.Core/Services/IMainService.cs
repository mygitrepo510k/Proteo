using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IMainService
    {
        Driver CurrentDriver { get; }
        Vehicle CurrentVehicle { get; }
        Models.Trailer CurrentTrailer { get; set; }
    }
}
