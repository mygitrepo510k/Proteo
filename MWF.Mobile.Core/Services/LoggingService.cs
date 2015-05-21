using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;

namespace MWF.Mobile.Core.Services
{
    public class LoggingService
       : ILoggingService
    {

        private readonly ILogMessageRepository _loggedRepository = null;
        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IGatewayService _gatewayService = null;
        private readonly IReachability _reachability = null;
        private readonly ICloseApplication _closeApplication = null;
        
        private MvxSubscriptionToken _notificationToken;


        public LoggingService(
            IRepositories repositories, 
            IGatewayService gatewayService, 
            IDeviceInfo deviceInfo,
            IReachability reachability,
            ICloseApplication closeApplication)
        {
            _loggedRepository = repositories.LogMessageRepository;
            _gatewayService = gatewayService;
            _deviceInfo = deviceInfo;
            _reachability = reachability;
            _closeApplication = closeApplication;

            _notificationToken = Mvx.Resolve<IMvxMessenger>().Subscribe<Messages.TopLevelExceptionHandlerMessage>(m =>
                LogAndClose(m.TopLevelException)
                );
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

                if (!_reachability.IsConnected())
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
                        Mvx.Resolve<ICustomUserInteraction>().Alert(message);
                        break;
                    }
                }
            }
        }

        private void LogAndClose(Exception ex)
        {
            LogEvent(ex);
            _closeApplication.CloseApp();
        }
    }
}
