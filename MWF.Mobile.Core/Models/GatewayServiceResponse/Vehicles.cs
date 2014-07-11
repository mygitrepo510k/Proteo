using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    [JsonConverter(typeof(JsonWrappedItemConverter<Vehicles>))]
    public class Vehicles
    {
        [JsonProperty("vehicle")]
        public List<Vehicle> List { get; set; }
    }

}
