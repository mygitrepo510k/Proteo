using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{   
    public class Item : IBlueSphereEntity
    {
        public Item()
        {
            ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        [XmlIgnore]
        public Guid ID { get; set; }

        [JsonProperty("description")]
        [XmlElement("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        [XmlElement("id")]
        public string ItemId { get; set; }

        [Ignore]
        [XmlIgnore]
        public string ItemIdFormatted { get { return ItemId.Replace("Order", ""); } }

        [JsonProperty("quantity")]
        [XmlElement("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("weight")]
        [XmlIgnore]
        public string Weight { get; set; }

        [JsonProperty("title")]
        [XmlElement("title")]
        public string Title { get; set; }

        [JsonProperty("deliveryordernumber")]
        [XmlIgnore]
        public string DeliveryOrderNumber { get; set; }

        [JsonProperty("businesstype")]
        [XmlIgnore]
        public string BusinessType { get; set; }

        [JsonProperty("deliverytype")]
        [XmlIgnore]
        public string GoodsType { get; set; }

        [ForeignKey(typeof(Order))]
        [XmlIgnore]
        [JsonIgnore]
        public Guid OrderId { get; set; }

        [ChildRelationship(typeof(ItemAdditional), RelationshipCardinality.OneToOne)]
        [JsonProperty("additional")]
        [XmlElement("additional")]
        public ItemAdditional Additional { get; set; }

        [JsonProperty("Barcode")]
        [XmlIgnore]
        public string Barcodes { get; set; }

    }
}