﻿using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class Vehicle : IBlueSphereEntity
    {
        [Unique]
        [JsonProperty("id")]
        public Guid ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("registration")]
        public string Registration { get; set; }

        [JsonProperty("safetyprofile")]
        public int SafetyCheckProfileIntLink { get; set; }

        [JsonProperty("istrailer")]
        public bool IsTrailer { get; set; }

    }
}
