﻿using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Address : IBlueSphereEntity
    {
        public Address()
        {
            ID = new Guid();
        }

        [Unique]
        [PrimaryKey]
        public Guid ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public InstructionType Type { get; set; }
        [JsonProperty("line")]
        public string[] Lines { get; set; }
        [JsonProperty("postcode")]
        public string Postcode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("arrive")]
        public DateTime Arrive { get; set; }
        [JsonProperty("depart")]
        public DateTime Depart { get; set; }

        [ForeignKey(typeof(Order))]
        public Guid OrderId { get; set; }
    }
}