﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface ILoggingService
    {
        Task LogEventAsync(Exception exception);
        Task LogEventAsync(string eventDescription, Enums.LogType type);
        Task LogEventAsync(string eventDescription, Enums.LogType type, params object[] args);
        Task UploadLoggedEventsAsync();
    }
}
