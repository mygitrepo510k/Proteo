using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{
    
    public class GatewayServiceTests
        :  MvxIoCSupportingTest
    {

        protected override void AdditionalSetup()
        {
            var mockDeviceInfoService = new Mock<Core.Services.IDeviceInfoService>();
            mockDeviceInfoService.SetupGet(m => m.DeviceIdentifier).Returns("021PROTEO0000001");
            mockDeviceInfoService.SetupGet(m => m.GatewayPassword).Returns("fleetwoodmobile");
            Ioc.RegisterSingleton<Core.Services.IDeviceInfoService>(mockDeviceInfoService.Object);
        }

        /// <summary>
        /// End-to-end test of gateway service.  Note this depends on a specific device existing in the database on BlueSphere.
        /// </summary>
        [Fact]
        public async Task GatewayService_GetDeviceReturnsCorrectDeviceID()
        {
            base.ClearAll();

            var service = new Core.Services.GatewayService(Ioc.Resolve<Core.Services.IDeviceInfoService>(), new Core.Services.HttpService());
            var device = await service.GetDevice();

            Assert.Equal(device.ID, new Guid("32de3ed3-ce3b-4a53-876d-442c351df668"));
        }

        /// <summary>
        /// Unit test with mocked http service
        /// </summary>
        [Fact]
        public async Task GatewayService_GetDriversReturnsList()
        {
            base.ClearAll();

            var testDriverID = Guid.NewGuid();

            // Mimic the response that comes back from the BlueSphere MWF Mobile gateway service - yes, it is convoluted!
            var responseActions = new[]
            {
                new Core.Models.GatewayServiceResponse.ResponseAction<Core.Models.GatewayServiceResponse.DriversWrapper>
                {
                    Data = new Core.Models.GatewayServiceResponse.DriversWrapper
                    {
                        Drivers = new Core.Models.GatewayServiceResponse.DriversInnerWrapper
                        {
                            List = new[] { new Core.Models.Driver { ID = testDriverID } }
                        }
                    }
                }
            };

            var response = new Core.HttpResult<Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.DriversWrapper>>
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Content = new Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.DriversWrapper> { Actions = responseActions },
            };

            var mockHttpService = new Mock<Core.Services.IHttpService>();
            mockHttpService.Setup(m => m.PostAsJsonAsync<Core.Models.GatewayServiceRequest.Content, Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.DriversWrapper>>(It.IsAny<Core.Models.GatewayServiceRequest.Content>(), It.IsAny<string>())).ReturnsAsync(response);

            var service = new Core.Services.GatewayService(Ioc.Resolve<Core.Services.IDeviceInfoService>(), mockHttpService.Object);
            var drivers = await service.GetDrivers();

            Assert.Equal(drivers.Count(), 1);
            Assert.Equal(drivers.First().ID, testDriverID);
        }

    }

}
