using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    public class DriversWrapper
    {
        [JsonProperty("drivers")]
        public DriversInnerWrapper Drivers { get; set; }
    }

    public class DriversInnerWrapper
    {
        [JsonProperty("driver")]
        public IEnumerable<Driver> List { get; set; }
    }

}
