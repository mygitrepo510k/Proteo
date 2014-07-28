using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class MobileApplicationData : IBlueSphereEntity
    {
        [JsonProperty(@"ID")]
        public Guid ID { get; set; }
        [JsonProperty(@"MobileApplicationId")]
        public Guid MobileApplicationId { get; set; }
        [JsonProperty(@"CustomerId")]
        public Guid CustomerId { get; set; }
        [JsonProperty(@"DeviceId")]
        public Guid DeviceId { get; set; }
        [JsonProperty(@"VehicleId")]
        public Guid VehicleId { get; set; }
        [JsonProperty(@"DriverId")]
        public Guid DriverId { get; set; }
        [JsonProperty(@"EffectiveDate")]
        public DateTime EffectiveDate { get; set; }
        public string Data { get; set; }
        [JsonProperty(@"SyncState")]
        public int SyncState { get; set; }
        public DateTime Created { get; set; }
        [JsonProperty(@"lock")]
        public bool Lock { get; set; }
        [JsonProperty(@"ondevice")]
        public bool OnDevice { get; set; }
        public bool IsDeleted { get; set; }
        [JsonProperty(@"vehicle")]
        public string VehicleRegistration { get; set; }
        [JsonProperty(@"driver")]
        public string DriverTitle { get; set; }
        [JsonProperty(@"last_activity")]
        public DateTime LastActvity { get; set; }
        [JsonProperty(@"static")]
        public bool Static { get; set; }
        public bool IsDone { get; set; }
        [JsonProperty(@"sequence")]
        public int Sequence { get; set; }
        [JsonProperty(@"group_title")]
        public string GroupTitle { get; set; }
        [JsonProperty(@"title")]
        public string Title { get; set; }
        [JsonProperty(@"group_subtitle")]
        public string GroupSubTitle { get; set; }
        [JsonProperty(@"subtitle")]
        public string SubTitle { get; set; }
    }

    public class Instruction
    {
        public List<string> Lines { get; set; }
    }

    public class Order
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty(@"id")]
        public string Id { get; set; }

        [JsonProperty(@"routeid")]
        public string RouteId { get; set; }

        [JsonProperty(@"title")]
        public string RouteTitle { get; set; }

        [JsonProperty(@"routedate")]
        public DateTime RouteDate { get; set; }

        [JsonProperty(@"sequence")]
        public int Sequence { get; set; }
        
        [JsonProperty(@"type")]
        public InstructionType Type { get; set; }
        
        [JsonProperty(@"priority")]
        public string Priority { get; set; }
        
        [JsonProperty(@"description")]
        public string Description { get; set; }
        
        [JsonProperty(@"description2")]
        public string Description2 { get; set; }

        public string OrderName { get; set; }

        [JsonProperty(@"arrive")]
        public DateTime Arrive { get; set; }

        [JsonProperty(@"depart")]
        public DateTime Depart { get; set; }

        public string RunType { get; set; }

        [JsonProperty(@"arrivedonsite")]
        public DateTime ArrivedOnSite { get; set; }

        [JsonProperty(@"completed")]
        public DateTime Completed { get; set; }

        public List<Contacts> Contacts { get; set; }
        
        [JsonProperty(@"addresses")]
        public List<Address> Addresses { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        [JsonProperty(@"instructions")]
        public List<JobInstruction> Instructions { get; set; }

        public List<Item> UnPlannedItems { get; set; }

        [JsonProperty(@"additional")]
        public List<Additional> Additionals { get; set; }

        [JsonProperty(@"additionalorder")]
        public Order[] AdditionalOrders { get; set; }

        [JsonProperty(@"activities")]
        public List<DriverActivity> DriverActivities { get; set; }
    }

    public class DriverActivity
    {
        public Guid DeviceId { get; set; }
        public string DeviceTitle { get; set; }
        public Guid DriverId { get; set; }
        public string DriverTitle { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleRegistration { get; set; }
        public Guid VehicleViewId { get; set; }
        public string VehicleViewTitle { get; set; }
        public Guid SecondaryVehicleId { get; set; }
        public string SecondaryVehicleTitle { get; set; }
        public string Code { get; set; }
        public string SubTitle { get; set; }
        public string Category { get; set; }
        public DateTime EffectiveDate { get; set; }
        public byte Activity { get; set; }
        public string Comment { get; set; }
        public string Smp { get; set; }
        public string Data { get; set; }
        public List<Signature> Signatures { get; set; }
        public int Sequence { get; set; }
        public ScannedDelivery ScannedDelivery { get; set; }
        public List<Picture> Pictures { get; set; }
    }

    public class Picture
    {
        public Guid Id { get; set; }
        public byte[] ImageData { get; set; }
        public string Sequance { get; set; }
    }

    public class ScannedDelivery
    {
        public string CustomerName { get; set; }
        public bool HasCustomerSigned { get; set; }
        public List<Barcode> Barcodes { get; set; }
    }

    public class OrderItem
    {
        public Order ParentOrder { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DeliveryOrderNumber { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string OrderId { get; set; }
        public string Quantity { get; set; }
        public string Weight { get; set; }
        public string Unit { get; set; }
        public string Barcode { get; set; }
        public string FilledIn { get; set; }
        public string Prince { get; set; }
        public bool BarcodeScanRequiredForCollection { get; set; }
        public bool BarcodeScanRequiredForDelivery { get; set; }
        public string DeliveryType { get; set; }
        public string BusinessType { get; set; }
        public List<Additional> Additionals { get; set; }
        public List<Barcode> Barcodes { get; set; }
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
        public List<Barcode> Barcodes { get; set; }
    }

    public class Barcode
    {
        public string BarcodeData { get; set; }
        public bool Scanned { get; set; }
        public bool Delivered { get; set; }
        public string OrderId { get; set; }
        public object Tag { get; set; }
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
