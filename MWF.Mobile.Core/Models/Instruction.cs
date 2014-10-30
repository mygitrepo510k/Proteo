using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models
{
    [XmlRoot("activity")]
    public class DriverActivity
    {

        public DriverActivity()
        {
            Id = Guid.NewGuid();
        }

        public DriverActivity(Driver currentDriver, Vehicle currentVehicle, Enums.DriverActivity currentActivity)
        {
            Id = Guid.NewGuid();
            DriverId = currentDriver.ID;
            VehicleId = currentVehicle.ID;
            Activity = (byte)currentActivity;
            var EffectiveDateTime = DateTime.Now;
            EffectiveDate = EffectiveDateTime.AddMilliseconds(-EffectiveDateTime.Millisecond);
        }

        [XmlAttribute("id")]
        public Guid Id { get; set; }

        [XmlAttribute("driverid")]
        public Guid DriverId { get; set; }

        [XmlIgnore]
        public string DriverTitle { get; set; }

        [XmlAttribute("vehicleid")]
        public Guid VehicleId { get; set; }

        [XmlIgnore]
        public string VehicleRegistration { get; set; }

        [XmlAttribute("vv_id")]
        public Guid VehicleViewId { get; set; }

        [XmlIgnore]
        public string VehicleViewTitle { get; set; }

        [XmlAttribute("v2_id")]
        public Guid SecondaryVehicleId { get; set; }

        [XmlIgnore]
        public string SecondaryVehicleTitle { get; set; }

        [XmlIgnore]
        public string Code { get; set; }

        [XmlIgnore]
        public string SubTitle { get; set; }

        [XmlIgnore]
        public string Category { get; set; }

        [XmlAttribute("effectivedate")]
        public DateTime EffectiveDate { get; set; }

        [XmlAttribute("activity")]
        public byte Activity { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; }

        [XmlAttribute("smp")]
        public string Smp { get; set; }

        [XmlAttribute("data")]
        public string Data { get; set; }

        [XmlIgnore]
        public List<Signature> Signatures { get; set; }

        [XmlIgnore]
        public int Sequence { get; set; }
        
        [XmlIgnore]
        public ScannedDelivery ScannedDelivery { get; set; }

        [XmlIgnore]
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
}
