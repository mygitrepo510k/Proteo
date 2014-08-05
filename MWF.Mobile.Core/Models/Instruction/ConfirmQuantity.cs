using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class ConfirmQuantity : IBlueSphereEntity
    {
        public ConfirmQuantity()
        {
            ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        public string Value { get; set; }
        [JsonProperty("@DisplayName")]
        public string DisplayName { get; set; }
        [JsonProperty("@type")]
        public int Type { get; set; }

        [ForeignKey(typeof(ItemAdditional))]
        public Guid ItemAdditionalId { get; set; }
    }
}