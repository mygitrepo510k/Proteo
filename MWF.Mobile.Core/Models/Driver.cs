using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{

    public class Driver : IBlueSphereEntity
    {

        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@intlink")]
        public int IntLink { get; set; }

        [JsonProperty("@firstname")]
        public string FirstName { get; set; }

        [JsonProperty("@lastname")]
        public string LastName { get; set; }

        [Ignore]
        public string DisplayName { get { return string.Format("{0} {1}", FirstName, LastName); } }

        public Guid LastVehicleID { get; set; }

        public Guid LastSecondaryVehicleID { get; set; } 

        public Guid LastVehicleViewID { get; set; }

        public Guid PhoneProfileID { get; set; }

        public DateTime LastLoggedOn { get; set; }

    }

}
