using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Enums;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
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

    public class Barcode
    {
        public string BarcodeData { get; set; }
        public bool Scanned { get; set; }
        public bool Delivered { get; set; }
        public string OrderId { get; set; }
        public object Tag { get; set; }
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
}
