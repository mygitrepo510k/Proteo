using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    // Model class which holds represents the results of a safety check performed by a driver on a vehicle
    public class SafetyCheckData: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@driverid")]
        public Guid DriverID { get; set; }

        [JsonProperty("@vehicleid")]
        public Guid VehicleID { get; set; }

        [JsonProperty("@vehicleregistration")]
        public string VehicleRegistration { get; set; }

        [JsonProperty("@driver")]
        public string DriverTitle { get; set; }

        [JsonProperty("@effectivedate")]
        public DateTime EffectiveDate { get; set; }

        [JsonProperty("@mileage")]
        public int Mileage { get; set; }

        [JsonProperty("@smp")]
        public string SMP { get; set; }

        [JsonProperty("@signature")]
        // Does this need a json wrapper?
        [ChildRelationship(typeof(Signature), RelationshipCardinality.OneToOne)]
        public Signature Signature { get; set; }

        [ChildRelationship(typeof(SafetyCheckFault))]
        [JsonProperty("faults")]
        [JsonConverter(typeof(JsonWrappedListConverter<SafetyCheckFault>))]
        public List<SafetyCheckFaultType> Faults { get; set; }

        [JsonProperty("@intlink")]
        public int ProfileIntLink { get; set; }


    }
}
