using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    [JsonConverter(typeof(JsonWrappedItemConverter<Drivers>))]
    public class Drivers
    {
        [JsonProperty("driver")]
        public List<Driver> List { get; set; }
    }

}
