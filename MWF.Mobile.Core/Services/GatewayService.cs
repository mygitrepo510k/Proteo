using MWF.Mobile.Core.Repositories;
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
        private readonly IDeviceRepository _deviceRepository;

        public GatewayService(IDeviceInfoService deviceInfoService, IHttpService httpService, IRepositories repositories)
        {
            _deviceInfoService = deviceInfoService;
            _httpService = httpService;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";
            _deviceRepository = repositories.DeviceRepository;
        }

        public async Task<Models.ApplicationProfile> GetApplicationProfile()
        {
            //TODO: work out what BlueSphere's doing here with the MobileApplicationProfileIntLink parameter
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "MobileApplicationProfileIntLink", Value = "0" } };
            var data = await ServiceCallAsync<Core.Models.ApplicationProfile>("fwGetApplicationProfile", parameters);
            return data.Result;
        }

        public async Task<Models.Device> GetDevice(string customerID)
        {
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "CustomerID", Value = customerID } };
            var data = await ServiceCallAsync<Core.Models.Device>("fwGetDevice", parameters);
            return data.Result;
        }

        public async Task<IEnumerable<Models.Driver>> GetDrivers()
        {
            var data = await ServiceCallAsync<Models.GatewayServiceResponse.Drivers>("fwGetDrivers");
            return data.Result.List;
        }

        public async Task<IEnumerable<Models.SafetyProfile>> GetSafetyProfiles()
        {
            var data = await ServiceCallAsync<Models.GatewayServiceResponse.SafetyProfiles>("fwGetSafetyProfiles");
            return data.Result.List;
        }

        public async Task<IEnumerable<Models.Vehicle>> GetVehicles(string vehicleViewTitle)
        {
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "VehicleView", Value = vehicleViewTitle} };
            var data = await ServiceCallAsync<Models.GatewayServiceResponse.Vehicles>("fwGetVehicles");
            return data.Result.List;
        }

        public async Task<IEnumerable<Models.VehicleView>> GetVehicleViews()
        {
            var data = await ServiceCallAsync<Models.GatewayServiceResponse.VehicleViews>("fwGetVehicleViews");
            return data.Result.List;
        }

        public async Task<Models.VerbProfile> GetVerbProfile(string verbProfileTitle)
        {
            var parameters = new[] { new Models.GatewayServiceRequest.Parameter { Name = "VerbProfileTitle", Value = verbProfileTitle } };
            var data = await ServiceCallAsync<Core.Models.VerbProfile>("fwGetVerbProfile", parameters);
            return data.Result;
        }

        private class ServiceCallResult<T>
        {
            public T Result { get; set; }
            public IEnumerable<string> Errors { get; set; }
        }

        private async Task<ServiceCallResult<T>> ServiceCallAsync<T>(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
            where T: class
        {
            var requestContent = CreateRequestContent(command, parameters);
            var response = await this.PostAsync<T>(requestContent);
            var responseActions = response.Content.Actions;

            if (!response.Succeeded || responseActions.Count() != 1)
                throw new Exception("No actions returned from Gateway service call.");

            var responseAction = responseActions.First();

            if (!responseAction.Ack)
                return new ServiceCallResult<T> { Result = default(T), Errors = responseAction.Errors };

            return new ServiceCallResult<T> { Result = responseAction.Data };
        }

        private Task<HttpResult<Models.GatewayServiceResponse.Response<TData>>> PostAsync<TData>(Models.GatewayServiceRequest.Content content)
            where TData: class
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
                DeviceIdentifier = _deviceRepository.GetAll().First().DeviceIdentifier,
                Password = _deviceInfoService.GatewayPassword,
                MobileApplication = _deviceInfoService.MobileApplication,
                Actions = actions,
            };
        }

    }

}
