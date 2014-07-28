using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Enums;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Order
    {
        [Unique]
        [PrimaryKey]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("routeid")]
        public string RouteId { get; set; }

        [JsonProperty("title")]
        public string RouteTitle { get; set; }

        [JsonProperty("routedate")]
        public DateTime RouteDate { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }
        
        [JsonProperty("type")]
        public InstructionType Type { get; set; }
        
        [JsonProperty("priority")]
        public string Priority { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("description2")]
        public string Description2 { get; set; }

        [JsonProperty("arrive")]
        public DateTime Arrive { get; set; }

        [JsonProperty("depart")]
        public DateTime Depart { get; set; }

        [JsonProperty("additional")]
        public Additional Additional { get; set; }
        
        [JsonProperty("addresses")]
        public List<Address> Addresses { get; set; }

        [JsonProperty("instructions")]
        public List<Instruction> Instructions { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}