﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class SafetyProfile
    {
        [Unique]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@intlink")]
        public int IntLink { get; set; }

        [JsonProperty("@odo")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool OdometerRequired { get; set; }

        [JsonProperty("@sig")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool SignatureRequired { get; set; }

        [JsonProperty("@checklist")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool DisplayAsChecklist { get; set; }

        [JsonProperty("@logon")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool DisplayAtLogon { get; set; }

        [JsonProperty("@logoff")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool DisplayAtLogoff { get; set; }

        [JsonProperty("@strailerprofile")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsTrailerProfile { get; set; }

        [Ignore]
        [JsonProperty("faults")]
        [JsonConverter(typeof(JsonWrappedListConverter<SafetyCheckFaultType>))]
        List<SafetyCheckFaultType> Faults { get; set; }

    }
}
