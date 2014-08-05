using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using MWF.Mobile.Core.Enums;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class MobileData : IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("@ID")]
        public Guid ID { get; set; }
        
        [JsonProperty("@CustomerId")]
        public Guid CustomerId { get; set; }
        
        [JsonProperty("@MobileApplicationId")]
        public Guid MobileApplicationId { get; set; }
        
        [JsonProperty("@DriverId")]
        public Guid DriverId { get; set; }
        
        [JsonProperty("@group_title")]
        public string GroupTitle { get; set; }

        [Ignore]
        public string GroupTitleFormatted
        {
            get { return (GroupTitle.StartsWith("Run")) ?  "Run - " + GroupTitle.Remove(0,3) : GroupTitle; }
        }
        
        [JsonProperty("@EffectiveDate")]
        public DateTime EffectiveDate { get; set; }
        
        [JsonProperty("@title")]
        public string Title { get; set; }
        
        [JsonProperty("@Reference")]
        public string Reference { get; set; }

        [JsonProperty("@SyncState")]
        public SyncState SyncState { get; set; }

        [JsonProperty("@sequence")]
        public int Sequence { get; set; }

        [ChildRelationship(typeof(Order), RelationshipCardinality.OneToOne)]
        [JsonProperty("order")]
        public Order Order { get; set; }

        public InstructionProgress ProgressState { get; set; }
    }
}