using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{
    [XmlRoot("Delivery_Description")]
    public class DeliveryDescription : IBlueSphereEntity
    {
        public DeliveryDescription()
        {
            ID = Guid.NewGuid();
        }

        [PrimaryKey]
        [Unique]
        [XmlIgnore]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        [XmlElement("")]
        public string Value { get; set; }
        [JsonProperty("@DisplayName")]
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }

        [ForeignKey(typeof(ItemAdditional))]
        [XmlIgnore]
        public Guid ItemAdditionalId { get; set; }
    }
}