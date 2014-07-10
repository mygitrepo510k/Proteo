using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    class VehicleExtractService : IVehicleExtractService
    {

        public Vehicle ExtractVehicle()
        {
            //TODO: This would be where you extract the vehicles from the database.

            return new Vehicle()
            {
                ID = System.Guid.NewGuid(),
                Registration = "EG11 ULT",
            };
        }
    }
}
