﻿using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class VerbProfileItem
    {
        [Unique]
        [JsonProperty("id")]
        public Guid ID { get; set; }

        [JsonProperty("tle")]
        public string Title { get; set; }

        [JsonProperty("seq")]
        public string Order { get; set; }

        [JsonProperty("hlt")]
        public bool IsHighlighted { get; set; }

        [JsonProperty("cde")]
        public string Code { get; set; }

        [JsonProperty("sig")]
        public bool ShowSignature { get; set; }

        [JsonProperty("com")]
        public bool ShowComment { get; set; }

        [JsonProperty("img")]
        public bool ShowImage { get; set; }

        [JsonProperty("cat")]
        public string Category { get; set; }

        [Indexed]
        public Guid VerbProfileID { get; set; }


    }
}
