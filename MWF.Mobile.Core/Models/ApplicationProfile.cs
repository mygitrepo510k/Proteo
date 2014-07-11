﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class ApplicationProfile : IBlueSphereEntity
    {
        [Unique]
        [JsonProperty("id")]
        public Guid ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("intlink")]
        public int IntLink { get; set; }

        [JsonProperty("poll")]
        public int PollingTime { get; set; }

        [JsonProperty("poll2")]
        public int PollingTime2 { get; set; }

        [JsonProperty("pollstart")]
        public int PollingStart { get; set; }

        [JsonProperty("pollstop")]
        public int PollingStop { get; set; }

        [JsonProperty("pollgprs")]
        public bool PollOnGPRS { get; set; }

        [JsonProperty("pollquantity")]
        public int PollingQuantity { get; set; }

        [JsonProperty("upload")]
        public int UploadTime { get; set; }

        [JsonProperty("upload2")]
        public int UploadTime2 { get; set; }

        [JsonProperty("uploadstart")]
        public int UploadStart { get; set; }

        [JsonProperty("uploadstop")]
        public int UploadStop { get; set; }

        [JsonProperty("uploadgprs")]
        public bool UploadGPRS { get; set; }

        [JsonProperty("uploadquantity")]
        public int UploadQuantity { get; set; }

        [JsonProperty("download")]
        public int DownloadTime { get; set; }

        [JsonProperty("download2")]
        public int DownloadTime2 { get; set; }

        [JsonProperty("downloadstart")]
        public int DownloadStart { get; set; }

        [JsonProperty("downloadstop")]
        public int DownloadStop { get; set; }

        [JsonProperty("downloadgprs")]
        public bool DownloadGPRS { get; set; }

        [JsonProperty("downloadquantity")]
        public int DownloadQuantity { get; set; }

        [JsonProperty("events")]
        public bool OnEvents { get; set; }

        [JsonProperty("events_delay")]
        public int OnEventsDelay { get; set; }

        [JsonProperty("retention")]
        public int DataRetention { get; set; }

        [JsonProperty("span")]
        public int DataSpan { get; set; }

        [JsonProperty("weekends")]
        public bool IncludeWeekends { get; set; }

        [JsonProperty("timeout")]
        public int Timeout { get; set; }

        [JsonProperty("instantmsg")]
        public bool UseInstantMessaging { get; set; }

        [JsonProperty("transactions")]
        public bool UseTransactions { get; set; }

    }
}
