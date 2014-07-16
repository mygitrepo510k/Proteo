using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{

    /// <summary>
    /// Base class for vehicle and trailer models
    /// </summary>
    public class BaseVehicle : IBlueSphereEntity
    {
        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@registration")]
        public string Registration { get; set; }

        [JsonProperty("@safetyprofile")]
        public int SafetyCheckProfileIntLink { get; set; }

        [JsonProperty("@istrailer")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsTrailer { get; set; }

    }
}
