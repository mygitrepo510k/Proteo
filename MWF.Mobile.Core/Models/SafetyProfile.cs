using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class SafetyProfile : IBlueSphereParentEntity<SafetyCheckFaultType>
    {
        [Unique]
        [JsonProperty("id")]
        public Guid ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("intlink")]
        public int IntLink { get; set; }

        [JsonProperty("odo")]
        public bool OdometerRequired { get; set; }

        [JsonProperty("sig")]
        public bool SignatureRequired { get; set; }

        [JsonProperty("checklist")]
        public bool DisplayAsChecklist { get; set; }

        [JsonProperty("logon")]
        public bool DisplayAtLogon { get; set; }

        [JsonProperty("logoff")]
        public bool DisplayAtLogoff { get; set; }

        [JsonProperty("strailerprofile")]
        public bool IsTrailerProfile { get; set; }

        [Ignore]
        public List<SafetyCheckFaultType> Children { get; set; }


    }
}
