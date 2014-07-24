using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace MWF.Mobile.Core.Models
{
    
    /// <summary>
    /// Stores the latest vehicle and trailer safety check for a driver
    /// </summary>
    public class LatestSafetyCheck : IBlueSphereEntity
    {

        public LatestSafetyCheck()
        {
            this.ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        public Guid ID { get; set; }

        [Unique]
        public Guid DriverID { get; set; }
        
        public SafetyCheckData VehicleSafetyCheck { get; set; }
        public SafetyCheckData TrailerSafetyCheck { get; set; }

    }

}
