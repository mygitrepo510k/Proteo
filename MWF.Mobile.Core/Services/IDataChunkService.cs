﻿using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IDataChunkService
    {
        Task SendReadChunkAsync(IEnumerable<MobileData> instructions, Driver currentDriver, Vehicle currentVehicle);
        Task SendDataChunkAsync(MobileApplicationDataChunkContentActivity dataChunkActivity, MobileData currentMobileData, Driver currentDriver, Vehicle currentVehicle, bool updateQuantity = false, bool updateTrailer = false);
    }
}
