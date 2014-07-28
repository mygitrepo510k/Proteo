using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Additional
    {
        [JsonProperty("trailerid")]
        public string TrailerId { get; set; }
        [JsonProperty("istrailerconfirmationenabled")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsTrailerConfirmationEnabled { get; set; }
        [JsonProperty("customernamerequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerNameRequiredForCollection { get; set; }
        [JsonProperty("customernamerequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerNameRequiredForDelivery { get; set; }
        [JsonProperty("customersignaturerequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerSignatureRequiredForCollection { get; set; }
        [JsonProperty("customersignaturerequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerSignatureRequiredForDelivery { get; set; }
    }
}