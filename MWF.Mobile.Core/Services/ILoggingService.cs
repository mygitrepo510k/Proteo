using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface ILoggingService
    {
        void LogEvent(Exception exception);
        void LogEvent(string eventDescription, Enums.LogType type);
        void LogEvent(string eventDescription, Enums.LogType type, params object[] args);
        Task UploadLoggedEventsAsync();
    }
}
