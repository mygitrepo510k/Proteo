using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Converters;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class ItemAdditional : IBlueSphereEntity
    {
        public ItemAdditional()
        {
            ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        public Guid ID { get; set; }

        [ChildRelationship(typeof(ConfirmQuantity), RelationshipCardinality.OneToOne)]
        [JsonProperty("Confirm_Quantity")]
        public ConfirmQuantity ConfirmQuantity { get; set; }

        [ChildRelationship(typeof(DeliveryDescription), RelationshipCardinality.OneToOne)]
        [JsonProperty("Delivery_Description")]
        public DeliveryDescription DeliveryDescription { get; set; }

        [JsonProperty("BarcodeScanRequiredForDelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForDelivery { get; set; }

        [JsonProperty("BarcodeScanRequiredForCollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForCollection { get; set; }

        [JsonProperty("BypassCleanClausedScreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCleanClausedScreen { get; set; }

        [JsonProperty("BypassCommentsScreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCommentsScreen { get; set; }

        [ForeignKey(typeof(Item))]
        public Guid ItemId { get; set; }
    }
}