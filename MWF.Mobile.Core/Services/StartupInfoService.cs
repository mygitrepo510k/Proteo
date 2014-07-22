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
        public Vehicle CurrentVehicle { get; set; }
        public Trailer CurrentTrailer { get; set; }
        public SafetyCheckData CurrentSafetyCheckData { get; set; }
        public int Mileage { get; set; }

		#region STUBS

        public SafetyCheckData VehicleSafetyCheckData
        {
            get
            {
                return new Models.SafetyCheckData
                {
                    DriverID = this.LoggedInDriver.ID,
                    DriverTitle = this.LoggedInDriver.Title,
                    VehicleID = this.CurrentVehicle.ID,
                    VehicleRegistration = this.CurrentVehicle.Registration,
                    EffectiveDate = DateTime.UtcNow,
                    Mileage = 0,
                    SMP = "838540DE98807598CB800300000000000000000000000",
                    ProfileIntLink = 477,
                };
            }
            set { }
        }

        public SafetyCheckData TrailerSafetyCheckData
        {
            get
            {
                if (this.CurrentTrailer == null)
                    return null;
                else
                    return new Models.SafetyCheckData
                    {
                        DriverID = this.LoggedInDriver.ID,
                        DriverTitle = this.LoggedInDriver.Title,
                        VehicleID = this.CurrentTrailer.ID,
                        VehicleRegistration = this.CurrentTrailer.Registration,
                        EffectiveDate = DateTime.UtcNow,
                        Mileage = 0,
                        SMP = "838540DE98807598CB800300000000000000000000000",
                        ProfileIntLink = 477,
                    };
            }
            set { }
        }

 		#endregion STUBS

    }

}
