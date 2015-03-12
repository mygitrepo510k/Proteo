using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public class LoggingService
       : ILoggingService
    {

        private readonly ILogMessageRepository _loggedRepository = null;
        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IGatewayService _gatewayService = null;

        public LoggingService(IRepositories repositories, IGatewayService gatewayService, IDeviceInfo deviceInfo)
        {
            _loggedRepository = repositories.LogMessageRepository;
            _gatewayService = gatewayService;
            _deviceInfo = deviceInfo;
        }

        public void LogEvent(Exception exception)
        {
            var loggedException = new LogMessage
            {
                LogDateTime = DateTime.Now.ToLocalTime(),
                Message = string.Format("{0} - {1}: {2}", Enums.LogType.Error.ToString(), exception.Message,exception.StackTrace),
                LogType = Enums.LogType.Error
            };

            _loggedRepository.Insert(loggedException);
        }

        public void LogEvent(string eventDescription, Enums.LogType type)
        {
            var loggedEvent = new LogMessage
            {
                LogDateTime = DateTime.Now.ToLocalTime(),
                Message = string.Format("{0} - {1}", type.ToString(), eventDescription),
                LogType = type
            };

            _loggedRepository.Insert(loggedEvent);
        }

        public async Task UploadLoggedEventsAsync()
        {
            var events = _loggedRepository.GetAll().OrderBy(e => e.LogDateTime).ToList();

            if (events.Any())
            {
                var reachability = Mvx.Resolve<IReachability>();

                if (!reachability.IsConnected())
                    return;

                var deviceIdentifier = _deviceInfo.GetDeviceIdentifier();
                

                foreach (var e in events)
                {
                    var deviceMessage = new DeviceLogMessage
                    {
                        Message = e.Message,
                        LogDateTime = e.LogDateTime,
                        DeviceIdentifier = deviceIdentifier
                    };

                    var response = await _gatewayService.PostLogMessageAsync(deviceMessage);

                    if (response.Succeeded)
                        _loggedRepository.Delete(e);
                    else
                    {
                        var message = "Error uploading log file.";
                        Mvx.Resolve<IUserInteraction>().Alert(message);
                        break;
                    }
                }
            }
        }
    }
}
