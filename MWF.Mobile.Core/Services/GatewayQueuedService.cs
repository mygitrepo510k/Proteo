using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Newtonsoft.Json;
using MWF.Mobile.Core.Repositories;

namespace MWF.Mobile.Core.Services
{
    
    public class GatewayQueuedService
        : IGatewayQueuedService
    {

        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IHttpService _httpService = null;
        private readonly Repositories.IGatewayQueueItemRepository _queueItemRepository = null;
        private readonly Portable.IReachability _reachability = null;
        private readonly IDeviceRepository _deviceRepository;

        private readonly IMvxMessenger _messenger = null;

        private readonly string _gatewayDeviceRequestUrl = null;

        private MvxSubscriptionToken _queueTimerMessageToken = null;
        private bool _isSubmitting = false;
        private bool _submitAgainOnCompletion = false;
        
        public GatewayQueuedService(IDeviceInfo deviceInfo, IHttpService httpService, Portable.IReachability reachability, IRepositories repositories, IMvxMessenger messenger)
        {
            _deviceInfo= deviceInfo;
            _httpService = httpService;
            _queueItemRepository = repositories.GatewayQueueItemRepository;
            _reachability = reachability;
            _messenger = messenger;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";

            _deviceRepository = repositories.DeviceRepository;
        }

        public void StartQueueTimer()
        {
            if (_queueTimerMessageToken == null)
                _queueTimerMessageToken = _messenger.Subscribe<Messages.GatewayQueueTimerElapsedMessage>(async m => await SubmitQueueAsync());

            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Start);
        }

        public void StopQueueTimer()
        {
            _queueTimerMessageToken.Dispose();
            _queueTimerMessageToken = null;

            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Stop);
        }

        /// <summary>
        /// Add a command to the queue and trigger submission to the MWF Mobile gateway web service
        /// </summary>
        /// <remarks>
        /// Note that submission will only occur if the GatewayQueueTimerService has been started (i.e. by first calling StartQueueTimer())
        /// </remarks>
        public void AddToQueueAndSubmit(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            this.AddToQueue(command, parameters);
            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Trigger);
        }

        public void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            var requestContent = CreateRequestContent(command, parameters);
            var serializedContent = JsonConvert.SerializeObject(requestContent);
            var queueItem = new Models.GatewayQueueItem { ID = Guid.NewGuid(), JsonSerializedRequestContent = serializedContent, QueuedDateTime = DateTime.Now };
            _queueItemRepository.Insert(queueItem);
        }

        private async Task SubmitQueueAsync()
        {
            Mvx.Trace("Submit Queue");

            if (_isSubmitting)
            {
                _submitAgainOnCompletion = true;
                return;
            }

            _isSubmitting = true;

            try
            {
                if (!_reachability.IsConnected())
                    return;

                var queuedItems = _queueItemRepository.GetAllInQueueOrder().ToList();

                foreach (var queuedItem in queuedItems)
                {
                    var submitted = true;

                    try
                    {
                        if (await this.ServiceCallAsync(queuedItem.JsonSerializedRequestContent))
                            _queueItemRepository.Delete(queuedItem);
                        else
                            //TODO: write failure to error log or report in some other way?
                            submitted = false;
                    }
                    catch
                    {
                        //TODO: write failure to error log or report in some other way?
                        submitted = false;
                    }

                    //TODO: should we attempt remaining items if one fails or bail out at this point?
                    if (!submitted)
                        break;
                }
            }
            finally
            {
                _isSubmitting = false;
            }

            if (_submitAgainOnCompletion)
            {
                _submitAgainOnCompletion = false;
                await SubmitQueueAsync();
            }
        }

        private async Task<bool> ServiceCallAsync(string jsonSerializedRequestContent)
        {
            var response = await this.PostAsync(jsonSerializedRequestContent);
            return response.Succeeded && response.Content.Actions.Any() && response.Content.Actions.All(a => a.Ack);
        }

        private Task<HttpResult<Models.GatewayServiceResponse.Response>> PostAsync(string jsonSerializedRequestContent)
        {
            return _httpService.PostAsync<Models.GatewayServiceResponse.Response>(jsonSerializedRequestContent, _gatewayDeviceRequestUrl);
        }

        /// <summary>
        /// Create a single-action request's content
        /// </summary>
        private Models.GatewayServiceRequest.Content CreateRequestContent(string command, IEnumerable<Models.GatewayServiceRequest.Parameter> parameters = null)
        {
            return this.CreateRequestContent(new[]
            {
                new Core.Models.GatewayServiceRequest.Action
                {
                    Command = command,
                    Parameters = parameters,
                }
            });
        }

        /// <summary>
        /// Create the request content, allowing multiple actions per request
        /// </summary>
        private Models.GatewayServiceRequest.Content CreateRequestContent(Models.GatewayServiceRequest.Action[] actions)
        {
            return new Core.Models.GatewayServiceRequest.Content
            {
                DeviceIdentifier = _deviceRepository.GetAll().First().DeviceIdentifier,
                Password = _deviceInfo.GatewayPassword,
                MobileApplication = _deviceInfo.MobileApplication,
                Actions = actions,
            };
        }

        private void PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand timerCommand)
        {
            _messenger.Publish(new Messages.GatewayQueueTimerCommandMessage(this, timerCommand));
        }

    }

}
