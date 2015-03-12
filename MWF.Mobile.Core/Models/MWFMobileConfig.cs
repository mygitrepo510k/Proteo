using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{
    public class MWFMobileConfig : IBlueSphereEntity
    {
        [PrimaryKey]
        [JsonProperty("CustomerID")]
        public Guid ID { get; set; }
        public bool ShowConfirmTrailerScreen { get; set; }
        public bool TrailerIsEditable { get; set; }
        public bool DeliveryAdd { get; set; }
        public string SafetyCheckPassText { get; set; }
        public string SafetyCheckFailText { get; set; }
        public string SafetyCheckDiscretionaryText { get; set; }
        public string HEUrl { get; set; }

    }
}
