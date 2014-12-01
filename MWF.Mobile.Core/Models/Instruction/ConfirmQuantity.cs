using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using System.Xml.Serialization;

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
        [XmlIgnore]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        [XmlText]
        public string Value { get; set; }

        [JsonProperty("@DisplayName")]
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("@type")]
        [XmlAttribute("type")]
        public int Type { get; set; }

        [ForeignKey(typeof(ItemAdditional))]
        [XmlIgnore]
        public Guid ItemAdditionalId { get; set; }
    }
}