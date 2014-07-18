using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    // Model class which holds an image associated with a safety check fault
    public class Signature: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@sequence")]
        public int Sequence { get; set; }

        [JsonProperty("@encoded")]
        public string EncodeImageData { get; set; }

        [ForeignKey(typeof(SafetyCheckFault))]
        public Guid SafetyCheckFaultID { get; set; }

    }
}
