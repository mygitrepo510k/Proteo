﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    public class DeviceWrapper
    {
        [JsonProperty("device")]
        public Device Device { get; set; }
    }

}
