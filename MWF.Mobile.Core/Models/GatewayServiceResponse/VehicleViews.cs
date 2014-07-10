using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    [JsonConverter(typeof(JsonWrappedItemConverter<VehicleViews>))]
    public class VehicleViews
    {
        [JsonProperty("vehicleview")]
        public List<VehicleView> List { get; set; }
    }

}
