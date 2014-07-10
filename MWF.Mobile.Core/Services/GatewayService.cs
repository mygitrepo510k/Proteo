using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    
    public class GatewayService
        : IGatewayService
    {

        private readonly IDeviceInfoService _deviceInfoService = null;
        private readonly IHttpService _httpService = null;
        private readonly string _gatewayDeviceRequestUrl = null;

        public GatewayService(IDeviceInfoService deviceInfoService, IHttpService httpService)
        {
            _deviceInfoService = deviceInfoService;
            _httpService = httpService;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";
        }

        public async Task<Models.Device> GetDevice()
        {
            var requestContent = CreateRequestContent(new[] { new Core.Models.GatewayServiceRequest.Action { Command = "fwGetDevice" } });
            var response = await this.CallGatewayServiceAsync<Core.Models.GatewayServiceResponse.DeviceWrapper>(requestContent);

            if (!response.Succeeded)
                return null;

            return response.Content.Actions.First().Data.Device;
        }

        public async Task<IEnumerable<Models.Driver>> GetDrivers()
        {
            var requestContent = CreateRequestContent(new[] { new Core.Models.GatewayServiceRequest.Action { Command = "fwGetDrivers" } });
            var response = await this.CallGatewayServiceAsync<Core.Models.GatewayServiceResponse.DriversWrapper>(requestContent);

            if (!response.Succeeded)
                return null;

            return response.Content.Actions.First().Data.Drivers.List;
        }

        private Task<HttpResult<Models.GatewayServiceResponse.Response<TData>>> CallGatewayServiceAsync<TData>(Models.GatewayServiceRequest.Content content)
            where TData : new()
        {
            return _httpService.PostAsJsonAsync<Models.GatewayServiceRequest.Content, Models.GatewayServiceResponse.Response<TData>>(content, _gatewayDeviceRequestUrl);
        }

        private Models.GatewayServiceRequest.Content CreateRequestContent(Models.GatewayServiceRequest.Action[] actions)
        {
            return new Core.Models.GatewayServiceRequest.Content
            {
                DeviceIdentifier = _deviceInfoService.DeviceIdentifier,
                Password = _deviceInfoService.GatewayPassword,
                Actions = actions,
            };
        }

    }

}
