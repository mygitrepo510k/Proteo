using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models 
{

    [JsonConverter(typeof(JsonWrappedItemConverter<VerbProfile>))]
    public class VerbProfile : IBlueSphereParentEntity<VerbProfileItem>
    {

        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@t")]
        public string Title { get; set; }

        [JsonProperty("@l")]
        public int IntLink { get; set; }

        [JsonProperty("@c")]
        public string Code { get; set; }

        [Ignore]
        [JsonProperty("vpis")]
        [JsonConverter(typeof(JsonWrappedListConverter<VerbProfileItem>))]
        public List<VerbProfileItem> Children { get; set; }

    }

}
