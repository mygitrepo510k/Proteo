using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class Instruction
    {
        public IList<string> Lines { get; set; }
    }

    public class Order : IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty(@"id")]
        public Guid ID { get; set; }
        [JsonProperty(@"sequence")]
        public int Sequence { get; set; }
        public InstructionType Type { get; set; }
        [JsonProperty(@"priority")]
        public string Priority { get; set; }
        [JsonProperty(@"description")]
        public string Description { get; set; }
        [JsonProperty(@"description2")]
        public string Description2 { get; set; }
        [JsonProperty(@"routeid")]
        public string RouteId { get; set; }
        [JsonProperty(@"title")]
        public string RouteTitle { get; set; }
        [JsonProperty(@"routedate")]
        public DateTime RouteDate { get; set; }

        public string OrderName { get; set; }

        [JsonProperty(@"arrive")]
        public DateTime Arrive { get; set; }

        [JsonProperty(@"depart")]
        public DateTime Depart { get; set; }

        public string RunType { get; set; }

        public DateTime ArrivedOnSite { get; set; }

        public DateTime Completed { get; set; }

        public IList<Contacts> Contacts { get; set; }

        [JsonProperty(@"additional")]
        public Additional Additional { get; set; }
        [JsonProperty(@"addresses")]
        public IList<Address> Addresses { get; set; }
        [JsonProperty(@"instructions")]
        public IList<JobInstruction> Instructions { get; set; }
    }

    public class Contacts
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Phone { get; set; }
    }

    public class JobInstruction
    {
        [JsonProperty(@"line")]
        public string Line { get; set; }
    }

    public class Item
    {
        [JsonProperty(@"id")]
        public string Id { get; set; }
        [JsonProperty(@"title")]
        public string Title { get; set; }
        [JsonProperty(@"description")]
        public string Description { get; set; }
        [JsonProperty(@"deliveryordernumber")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool DeliveryOrderNumber { get; set; }
        [JsonProperty(@"customerordernumber")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerOrderNumber { get; set; }
        [JsonProperty(@"orderid")]
        public int OrderId { get; set; }
        [JsonProperty(@"quantity")]
        public decimal Quantity { get; set; }
        [JsonProperty(@"weight")]
        public int Weight { get; set; }
        [JsonProperty(@"businesstype")]
        public BusinessType BusinessType { get; set; }
        [JsonProperty(@"deliverytype")]
        public DeliveryType DeliveryType { get; set; }
        [JsonProperty(@"additional")]
        public ItemAdditional Additional { get; set; }
    }

    public class ItemAdditional
    {
        [JsonProperty(@"barcodescanrequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForCollection { get; set; }
        [JsonProperty(@"barcodescanrequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BarcodeScanRequiredForDelivery { get; set; }
        [JsonProperty(@"bypasscleanclausedscreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCleanClausedScreen { get; set; }
        [JsonProperty(@"bypasscommentsscreen")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool BypassCommentsScreen { get; set; }
        [JsonProperty(@"confirmquality")]
        public decimal ConfirmQuality { get; set; }
        [JsonProperty(@"barcodes")]
        public IList<Barcode> Barcodes { get; set; }
    }

    public class Barcode
    {
    }

    public enum DeliveryType
    {
        Normal
    }

    public enum BusinessType
    {
        Dedicated24
    }

    public class Address
    {
        [JsonProperty(@"title")]
        public string Title { get; set; }
        [JsonProperty(@"type")]
        public InstructionType Type { get; set; }
        [JsonProperty(@"line")]
        public string Line1 { get; set; }
        [JsonProperty(@"line")]
        public string Line2 { get; set; }
        [JsonProperty(@"line")]
        public string Line3 { get; set; }
        [JsonProperty(@"line")]
        public string Line4 { get; set; }
        [JsonProperty(@"postcode")]
        public string Postcode { get; set; }
        [JsonProperty(@"country")]
        public string Country { get; set; }
        [JsonProperty(@"arrive")]
        public DateTime Arrive { get; set; }
        [JsonProperty(@"depart")]
        public DateTime Depart { get; set; }
    }

    public class Additional
    {
        [JsonProperty(@"trailerid")]
        public string TrailerId { get; set; }
        [JsonProperty(@"istrailerconfirmationenabled")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsTrailerConfirmationEnabled { get; set; }
        [JsonProperty(@"customernamerequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerNameRequiredForCollection { get; set; }
        [JsonProperty(@"customernamerequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerNameRequiredForDelivery { get; set; }
        [JsonProperty(@"customersignaturerequiredforcollection")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerSignatureRequiredForCollection { get; set; }
        [JsonProperty(@"customersignaturerequiredfordelivery")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool CustomerSignatureRequiredForDelivery { get; set; }
    }

    public enum InstructionType
    {
        Collect = 1,
        Deliver = 2
    }
}
