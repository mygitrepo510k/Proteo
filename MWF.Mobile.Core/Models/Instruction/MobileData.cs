using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using MWF.Mobile.Core.Enums;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{
    [XmlRoot("mobiledata")]
    public class MobileData : IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [XmlAttribute("ID")]
        [JsonProperty("@ID")]
        public Guid ID { get; set; }

        [XmlAttribute("CustomerID")]
        [JsonProperty("@CustomerId")]
        public Guid CustomerId { get; set; }

        [XmlAttribute("MobileApplicationID")]
        [JsonProperty("@MobileApplicationId")]
        public Guid MobileApplicationId { get; set; }

        [XmlAttribute("DriverID")]
        [JsonProperty("@DriverId")]
        public Guid DriverId { get; set; }

        [XmlAttribute("VehicleID")]
        [JsonProperty("@VehicleId")]
        public Guid VehicleId { get; set; }

        [XmlAttribute("group_title")]
        [JsonProperty("@group_title")]
        public string GroupTitle { get; set; }

        [Ignore]
        [XmlIgnore]
        public string GroupTitleFormatted
        {
            get { return (GroupTitle.StartsWith("Run")) ?  "Run - " + GroupTitle.Remove(0,3) : GroupTitle; }
        }

        [XmlAttribute("EffectiveDate")]
        [JsonProperty("@EffectiveDate")]
        public DateTime EffectiveDate { get; set; }

        [XmlAttribute("title")]
        [JsonProperty("@title")]
        public string Title { get; set; }

        [XmlAttribute("Reference")]
        [JsonProperty("@Reference")]
        public string Reference { get; set; }

        [Ignore]
        [XmlAttribute("SyncState")]
        public int SyncStateInt
        {
            get { return (int)SyncState; }
            set { SyncState = (Enums.SyncState)value; }
        }
        
        [XmlIgnore]
        [JsonProperty("@SyncState")]
        public SyncState SyncState { get; set; }

        [XmlAttribute("sequence")]
        [JsonProperty("@sequence")]
        public int Sequence { get; set; }

        [ChildRelationship(typeof(Order), RelationshipCardinality.OneToOne)]
        [XmlIgnore]
        [JsonProperty("order")]
        public Order Order { get; set; }

        [XmlIgnore]
        public InstructionProgress ProgressState { get; set; }

        [XmlIgnore]
        public int LatestDataChunkSequence { get; set; }

        [XmlIgnore]
        public string MessageText 
        {
            get
            {
                return (this.Order.Addresses == null)
                    ? string.Format("{0} ({1})", this.Order.Items[0].Description, "Message")
                    : string.Format("{0} ({1})", this.Order.Description, "Message with Point");
            }
        }

        public override string ToString()
        {
            return this.Reference;
        }


    }
}