using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class DeliveryDescription : IBlueSphereEntity
    {
        public DeliveryDescription()
        {
            ID = new Guid();
        }

        [PrimaryKey]
        [Unique]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        public string Value { get; set; }
        [JsonProperty("@DisplayName")]
        public string DisplayName { get; set; }

        [ForeignKey(typeof(ItemAdditional))]
        public Guid ItemAdditionalId { get; set; }
    }
}