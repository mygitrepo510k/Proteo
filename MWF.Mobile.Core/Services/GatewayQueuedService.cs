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
        private readonly string _gatewayDeviceRequestUrl = null;

        public GatewayQueuedService(IDeviceInfoService deviceInfoService, IHttpService httpService)
        {
            _deviceInfoService = deviceInfoService;
            _httpService = httpService;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";
        }

        public async Task<bool> AddToQueueAndSubmitAsync(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            await AddToQueueAsync(command, parameters);
            return await SubmitQueueAsync();
        }

        public async Task AddToQueueAsync(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            var requestContent = CreateRequestContent(command, parameters);
            var serializedContent = JsonConvert.SerializeObject(requestContent);
            var queueItem = new Models.GatewayQueueItem { JsonSerializedRequestContent = serializedContent, QueuedDateTime = DateTime.Now };
        }

        private async Task<bool> SubmitQueueAsync()
        {
            var allItemsSubmitted = true;
            var queuedItems = Enumerable.Empty<Models.GatewayQueueItem>(); //TODO: retrieve queued items from database

            //TODO: handle timeouts and lack of signal

            foreach (var queuedItem in queuedItems)
            {
                if (await this.ServiceCallAsync(queuedItem.JsonSerializedRequestContent))
                {
                    //TODO: delete from queue? mark as sent?
                }
                else
                {
                    //TODO: should we attempt remaining items if one fails or bail out at this point
                    //TODO: write failure to error log or report in some other way?
                    allItemsSubmitted = false;
                }
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
