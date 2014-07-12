﻿using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    public class SafetyCheckFaultType : IBlueSphereEntity
    {
        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@category")]
        public string Category { get; set; }

        [JsonProperty("@order")]
        public int Order { get; set; }

        [JsonProperty("@highlight")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsHighlighted { get; set; }

        [ForeignKey(typeof(SafetyProfile))]
        public Guid SafetyProfileID { get; set; }

    }
}
