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

        private readonly string _mwfCustomerId = "C697166B-2E1B-45B0-8F77-270C4EADC099";

        protected override void AdditionalSetup()
        {
            var mockDeviceInfoService = new Mock<Core.Services.IDeviceInfoService>();
            mockDeviceInfoService.SetupGet(m => m.DeviceIdentifier).Returns("021PROTEO0000001");
            mockDeviceInfoService.SetupGet(m => m.GatewayPassword).Returns("fleetwoodmobile");
            mockDeviceInfoService.SetupGet(m => m.MobileApplication).Returns("Orchestrator");
            Ioc.RegisterSingleton<Core.Services.IDeviceInfoService>(mockDeviceInfoService.Object);
        }



        /// <summary>
        /// End-to-end test of gateway service.  Note this depends on the "021PROTEO0000001" device existing in the database on BlueSphere.
        /// </summary>
        [Fact]
        public async Task GatewayService_EndToEnd_GetDeviceReturnsCorrectID()
        {
            base.ClearAll();

            Ioc.RegisterSingleton<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = Ioc.IoCConstruct<Core.Services.GatewayService>();
            var device = await service.GetDevice(_mwfCustomerId);

            Assert.Equal(device.ID, new Guid("32de3ed3-ce3b-4a53-876d-442c351df668"));
        }

        /// <summary>
        /// End-to-end test of gateway service.  Note this depends on the "Palletforce" verb profile existing in the database on BlueSphere.
        /// </summary>
        [Fact]
        public async Task GatewayService_EndToEnd_GetVerbProfileReturnsCorrectData()
        {
            base.ClearAll();

            Ioc.RegisterSingleton<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = Ioc.IoCConstruct<Core.Services.GatewayService>();
            var verbProfile = await service.GetVerbProfile("Palletforce");

            Assert.Equal(verbProfile.ID, new Guid("d7af04cd-1857-42f1-a8c1-d019bd1d6223"));
            Assert.Equal(verbProfile.Children.Count(), 2);
            Assert.Equal(verbProfile.Children.First().ID, new Guid("3cdb1c6b-1997-423f-a9ea-08b62bee9a30"));
        }

        /// <summary>
        /// End-to-end test of gateway service.
        /// </summary>
        [Fact]
        public async Task GatewayService_EndToEnd_GetSafetyProfilesReturnsData()
        {
            base.ClearAll();

            Ioc.RegisterSingleton<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = Ioc.IoCConstruct<Core.Services.GatewayService>();
            var safetyProfiles = await service.GetSafetyProfiles();

            Assert.NotEqual(safetyProfiles.Count(), 0);
        }

        /// <summary>
        /// Unit test with mocked http service
        /// </summary>
        [Fact]
        public async Task GatewayService_GetDriversReturnsList()
        {
            base.ClearAll();

            var testDriverID = Guid.NewGuid();

            // Mimic the response that comes back from the BlueSphere MWF Mobile gateway service
            var responseActions = new[]
            {
                new Core.Models.GatewayServiceResponse.ResponseAction<Core.Models.GatewayServiceResponse.Drivers>
                {
                    Ack = true,
                    Data = new Core.Models.GatewayServiceResponse.Drivers
                    {
                        List = new List<Core.Models.Driver> { new Core.Models.Driver { ID = testDriverID } }
                    }
                }
            };

            var response = new Core.HttpResult<Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.Drivers>>
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Content = new Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.Drivers> { Actions = responseActions },
            };

            var mockHttpService = new Mock<Core.Services.IHttpService>();
            mockHttpService.Setup(m => m.PostAsJsonAsync<Core.Models.GatewayServiceRequest.Content, Core.Models.GatewayServiceResponse.Response<Core.Models.GatewayServiceResponse.Drivers>>(It.IsAny<Core.Models.GatewayServiceRequest.Content>(), It.IsAny<string>())).ReturnsAsync(response);
            Ioc.RegisterSingleton<Core.Services.IHttpService>(mockHttpService.Object);

            var service = Ioc.IoCConstruct<Core.Services.GatewayService>();
            var drivers = await service.GetDrivers();

            Assert.Equal(drivers.Count(), 1);
            Assert.Equal(drivers.First().ID, testDriverID);
        }


    }

}
