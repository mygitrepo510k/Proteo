using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class SafetyCheckFaultType
    {
        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@category")]
        public string Category { get; set; }

        [JsonProperty("@order")]
        public int Order { get; set; }

        [JsonProperty("@highlight")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsHighlighted { get; set; }

        public Guid SafetyProfileID { get; set; }

    }
}
