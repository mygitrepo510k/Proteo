using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Trailer : IBlueSphereEntity
    {
        public Trailer()
        {
            ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        public string TrailerId { get; set; }

        [JsonProperty("@DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [ForeignKey(typeof(Additional))]
        public Guid AdditionalId { get; set; }
    }
}
