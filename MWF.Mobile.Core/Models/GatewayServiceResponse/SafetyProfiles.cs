using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    [JsonConverter(typeof(JsonWrappedItemConverter<SafetyProfiles>))]
    public class SafetyProfiles
    {
        [JsonProperty("safetyprofile")]
        public List<SafetyProfile> List { get; set; }
    }

}
