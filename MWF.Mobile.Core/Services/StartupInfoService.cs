using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{

    //Simple class for in-memory storage of information picked up during the startup process
    public class StartupInfoService : IStartupInfoService
    {

        public Driver LoggedInDriver {get; set;}
        public SafetyCheckData CurrentVehicleSafetyCheckData { get; set; }
        public SafetyCheckData CurrentTrailerSafetyCheckData { get; set; }
        public Vehicle CurrentVehicle { get; set; }
        public Trailer CurrentTrailer { get; set; }
        public int Mileage { get; set; }

    }

}
