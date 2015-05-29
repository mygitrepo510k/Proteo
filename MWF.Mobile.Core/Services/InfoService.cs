using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{

    // Simple class for in-memory storage of useful information e.g. current driver, vehicle, trailer etc
    public class InfoService
        : IInfoService
    {

        public Driver LoggedInDriver { get; set; }
        public Vehicle CurrentVehicle { get; set; }
        public Trailer CurrentTrailer { get; set; }
        public int Mileage { get; set; }
    }

}
