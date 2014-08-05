using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

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
        public Guid ID { get; set; }

        [ChildRelationship(typeof(ItemAdditional), RelationshipCardinality.OneToOne)]
        [JsonProperty("additional")]
        public ItemAdditional Additional { get; set; }
        [JsonProperty("Barcodes")]
        public string Barcodes { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("id")]
        public string ItemId { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        [ForeignKey(typeof(Order))]
        public Guid OrderId { get; set; }
    }
}