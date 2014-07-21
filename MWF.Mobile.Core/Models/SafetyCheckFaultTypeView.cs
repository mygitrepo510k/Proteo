using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{
    public class SafetyCheckFaultTypeView : IBlueSphereEntity
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

        [ForeignKey(typeof(SafetyProfile))]
        public Guid SafetyProfileID { get; set; }

        public bool IsPassed { get; set; }
    }
}
