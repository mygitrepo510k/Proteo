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

        public Task<Models.ApplicationProfile> GetApplicationProfile()
        {
            return GetData<Core.Models.ApplicationProfile>("fwGetApplicationProfile");
        }

        public Task<Models.Device> GetDevice()
        {
            return GetData<Core.Models.Device>("fwGetDevice");
        }

        public async Task<IEnumerable<Models.Driver>> GetDrivers()
        {
            var data = await GetData<Models.GatewayServiceResponse.Drivers>("fwGetDrivers");
            return data.List;
        }

        public async Task<IEnumerable<Models.SafetyProfile>> GetSafetyProfiles()
        {
            var data = await GetData<Models.GatewayServiceResponse.SafetyProfiles>("fwGetSafetyProfiles");
            return data.List;
        }

        public async Task<IEnumerable<Models.Vehicle>> GetVehicles(string vehicleViewTitle)
        {
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "VehicleView", Value = vehicleViewTitle} };
            var data = await GetData<Models.GatewayServiceResponse.Vehicles>("fwGetVehicles");
            return data.List;
        }

        public async Task<IEnumerable<Models.VehicleView>> GetVehicleViews()
        {
            var data = await GetData<Models.GatewayServiceResponse.VehicleViews>("fwGetVehicleViews");
            return data.List;
        }

        public Task<Models.VerbProfile> GetVerbProfile(string verbProfileTitle)
        {
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "VerbProfileTitle", Value = verbProfileTitle } };
            return GetData<Core.Models.VerbProfile>("fwGetVerbProfile", parameters);
        }

        private async Task<T> GetData<T>(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            var requestContent = CreateRequestContent(command, parameters);
            var response = await this.CallGatewayServiceAsync<T>(requestContent);

            if (!response.Succeeded)
                //TODO: should we throw an exception here or something?
                return default(T);

            return response.Content.Actions.First().Data;
        }

        private Task<HttpResult<Models.GatewayServiceResponse.Response<TData>>> CallGatewayServiceAsync<TData>(Models.GatewayServiceRequest.Content content)
        {
            return _httpService.PostAsJsonAsync<Models.GatewayServiceRequest.Content, Models.GatewayServiceResponse.Response<TData>>(content, _gatewayDeviceRequestUrl);
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
