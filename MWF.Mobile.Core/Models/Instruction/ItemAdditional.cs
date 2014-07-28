using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class ItemAdditional
    {
        [JsonProperty(@"barcodescanrequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForCollection { get; set; }
        [JsonProperty(@"barcodescanrequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForDelivery { get; set; }
        [JsonProperty(@"bypasscleanclausedscreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCleanClausedScreen { get; set; }
        [JsonProperty(@"bypasscommentsscreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCommentsScreen { get; set; }
        [JsonProperty(@"confirmquality")]
        public decimal ConfirmQuality { get; set; }
    }
}