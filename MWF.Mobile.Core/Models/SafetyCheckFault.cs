using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    // Model class which represents a fault for a specific safety check item 9SafetyCheckFaultType)
    public class SafetyCheckFault: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@faultid")]
        public Guid FaultTypeID { get; set; }

        [JsonProperty("@reference")]
        public string FaultTypeReference { get; set; }

        [JsonProperty("@comment")]
        public string Comment { get; set; }

        [JsonProperty("@discrete")]
        public bool IsDiscretionaryPass { get; set; }

        [ChildRelationship(typeof(Image))]
        [JsonProperty("image")]
        [JsonConverter(typeof(JsonWrappedListConverter<Image>))]
        public List<Image> Images { get; set; }

        [ForeignKey(typeof(SafetyCheckData))]
        public Guid SafetyCheckDataID { get; set; }

    }
}
