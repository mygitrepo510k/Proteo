using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface ILoggingService
    {
        void LogException(Exception exception);
        Task UploadLoggedExceptionsAsync();
    }
}
