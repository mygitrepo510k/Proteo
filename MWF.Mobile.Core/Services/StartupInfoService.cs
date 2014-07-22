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

        public Driver LoggedInDriver { get; set; }

		#region STUBS

        public Vehicle Vehicle
        {
            get { return new Vehicle { ID = Guid.Parse("388D081A-1891-47D4-A27B-089CAA3A8BA7"), Registration = "TEST REG1" }; }
            set { }
        }

        public Trailer Trailer
        {
            get { return new Trailer { ID = Guid.Parse("AACC35D8-68C5-438E-9997-13021E9C40F4"), Registration = "Test Trailer2" }; }
            set { }
        }

        public SafetyCheckData VehicleSafetyCheckData
        {
            get
            {
                return new Models.SafetyCheckData
                {
                    DriverID = this.LoggedInDriver.ID,
                    DriverTitle = this.LoggedInDriver.Title,
                    VehicleID = this.Vehicle.ID,
                    VehicleRegistration = this.Vehicle.Registration,
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
                return new Models.SafetyCheckData
                {
                    DriverID = this.LoggedInDriver.ID,
                    DriverTitle = this.LoggedInDriver.Title,
                    VehicleID = this.Trailer.ID,
                    VehicleRegistration = this.Trailer.Registration,
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
