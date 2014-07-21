using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    // Model class which holds the driver signature for a safety check
    public class Image: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@encoded")]
        public string EncodedSignature { get; set; }

        [JsonProperty("@encodedimage")]
        public string EncodedImage { get; set; }

        [ForeignKey(typeof(SafetyCheckData))]
        public Guid SafetyCheckDataID { get; set; }

    }
}
