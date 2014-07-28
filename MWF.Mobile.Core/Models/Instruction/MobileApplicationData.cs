using System;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class MobileApplicationData : IBlueSphereEntity
    {
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
        
        [JsonProperty("@EffectiveDate")]
        public DateTime EffectiveDate { get; set; }
        
        [JsonProperty("@title")]
        public string Title { get; set; }
        
        [JsonProperty("@Reference")]
        public string Reference { get; set; }

        [JsonProperty("@SyncState")]
        public int SyncState { get; set; }

        [JsonProperty("@sequence")]
        public int Sequence { get; set; }

        [JsonProperty("@DeviceId")]
        public Guid DeviceId { get; set; }
        
        [JsonProperty("@VehicleId")]
        public Guid VehicleId { get; set; }
        
        [JsonProperty("@lock")]
        public bool Lock { get; set; }

        [JsonProperty("@ondevice")]
        public bool OnDevice { get; set; }

        public bool IsDeleted { get; set; }

        [JsonProperty("@vehicle")]
        public string VehicleRegistration { get; set; }

        [JsonProperty("@driver")]
        public string DriverTitle { get; set; }

        [JsonProperty("@last_activity")]
        public DateTime LastActvity { get; set; }

        [JsonProperty(@"static")]
        public bool Static { get; set; }

        public bool IsDone { get; set; }

        [JsonProperty(@"group_subtitle")]
        public string GroupSubTitle { get; set; }

        [JsonProperty(@"subtitle")]
        public string SubTitle { get; set; }

        [JsonProperty("@order")]
        public Order Order { get; set; }
    }
}