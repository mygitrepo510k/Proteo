using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Services
{
    
    public class GatewayQueuedService
        : IGatewayQueuedService
    {

        private readonly IDeviceInfoService _deviceInfoService = null;
        private readonly IHttpService _httpService = null;
        private readonly Repositories.IGatewayQueueItemRepository _queueItemRepository = null;
        private readonly Portable.IReachability _reachability = null;

        private readonly string _gatewayDeviceRequestUrl = null;

        public GatewayQueuedService(IDeviceInfoService deviceInfoService, IHttpService httpService, Repositories.IGatewayQueueItemRepository queueItemRepository, Portable.IReachability reachability)
        {
            _deviceInfoService = deviceInfoService;
            _httpService = httpService;
            _queueItemRepository = queueItemRepository;
            _reachability = reachability;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";
        }

        public Task<bool> AddToQueueAndSubmitAsync(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            this.AddToQueue(command, parameters);
            return this.SubmitQueueAsync();
        }

        public void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            var requestContent = CreateRequestContent(command, parameters);
            var serializedContent = JsonConvert.SerializeObject(requestContent);
            var queueItem = new Models.GatewayQueueItem { ID = Guid.NewGuid(), JsonSerializedRequestContent = serializedContent, QueuedDateTime = DateTime.Now };
            _queueItemRepository.Insert(queueItem);
        }

        private async Task<bool> SubmitQueueAsync()
        {
            if (!_reachability.IsConnected())
                return false;

            var allItemsSubmitted = true;
            var queuedItems = _queueItemRepository.GetAllInQueueOrder().ToList();

            foreach (var queuedItem in queuedItems)
            {
                try
                {
                    if (await this.ServiceCallAsync(queuedItem.JsonSerializedRequestContent))
                        _queueItemRepository.Delete(queuedItem);
                    else
                        //TODO: write failure to error log or report in some other way?
                        allItemsSubmitted = false;
                }
                catch
                {
                    //TODO: write failure to error log or report in some other way?
                    allItemsSubmitted = false;
                }

                //TODO: should we attempt remaining items if one fails or bail out at this point?
                if (!allItemsSubmitted)
                    break;
            }

            return allItemsSubmitted;
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
                DeviceIdentifier = _deviceInfoService.DeviceIdentifier,
                Password = _deviceInfoService.GatewayPassword,
                MobileApplication = _deviceInfoService.MobileApplication,
                Actions = actions,
            };
        }

    }

}
