﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using Xunit;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Tests.ServiceTests
{
    
    public class GatewayServiceTests
        :  MvxIoCSupportingTest
    {

        private readonly string _mwfCustomerID = "C697166B-2E1B-45B0-8F77-270C4EADC031";

        private IFixture _fixture;
        private Mock<IGatewayQueueItemRepository> _mockQueueItemRepository;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockQueueItemRepository = new Mock<Core.Repositories.IGatewayQueueItemRepository>();
            var mockDeviceInfo = new Mock<Core.Services.IDeviceInfo>();
            mockDeviceInfo.SetupGet(m => m.GatewayPassword).Returns("fleetwoodmobile");
            mockDeviceInfo.SetupGet(m => m.MobileApplication).Returns("Orchestrator");
            _fixture.Inject<Core.Services.IDeviceInfo>(mockDeviceInfo.Object);
            
            //_fixture.Register<Core.Services.IDeviceInfoService>(mockDeviceInfoService.Object);




            List<Device> devices = new List<Device>() { new Device() { DeviceIdentifier = "021PROTEO0000001" } };
            IDeviceRepository repo = Mock.Of<IDeviceRepository>(dr => dr.GetAll() == devices);
            IRepositories repos = Mock.Of<IRepositories>(r => r.DeviceRepository == repo &&
                                                              r.GatewayQueueItemRepository == _mockQueueItemRepository.Object);
            _fixture.Register<IRepositories>(() => repos);


        }

        /// <summary>
        /// End-to-end test of gateway service.  Note this depends on the "021PROTEO0000001" device existing in the database on BlueSphere.
        /// </summary>
        [Fact]
        public async Task GatewayService_EndToEnd_GetDeviceReturnsCorrectID()
        {
            base.ClearAll();

            _fixture.Inject<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = _fixture.Create<Core.Services.GatewayService>();
            var device = await service.GetDevice(_mwfCustomerID);

            Assert.Equal(device.ID, new Guid("32de3ed3-ce3b-4a53-876d-442c351df668"));
        }

        /// <summary>
        /// End-to-end test of gateway service.  Note this depends on the "Palletforce" verb profile existing in the database on BlueSphere.
        /// </summary>
        [Fact]
        public async Task GatewayService_EndToEnd_GetVerbProfileReturnsCorrectData()
        {
            base.ClearAll();

            _fixture.Inject<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = _fixture.Create<Core.Services.GatewayService>();
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

            _fixture.Inject<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = _fixture.Create<Core.Services.GatewayService>();
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

            // Mimic the response that comes back from the MWF Mobile gateway service
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
            _fixture.Inject<Core.Services.IHttpService>(mockHttpService.Object);

            var service = _fixture.Create<Core.Services.GatewayService>();
            var drivers = await service.GetDrivers();

            Assert.Equal(drivers.Count(), 1);
            Assert.Equal(drivers.First().ID, testDriverID);
        }

        /// <summary>
        /// End to end test of gateway service GetDriverInstruction method 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GatewayService_EndToEnd_GetDriverInstructions()
        {
            base.ClearAll();

            /*
             * This test works with the BlueSphere database so adding vehicles to this Driver (Test, Test) 
             * will affect the test and cause it to fail.
             */

            Guid driverId = Guid.Parse("ABE51028-5AB2-4753-8E34-67F5C86F9E77");
            _fixture.Inject<Core.Services.IHttpService>(new Core.Services.HttpService());
            var service = _fixture.Create<Core.Services.GatewayService>();
            var driverInstructions = await service.GetDriverInstructions("004", driverId, DateTime.Now.AddYears(-1), DateTime.Now);

            Assert.Equal(2, driverInstructions.Count());
            Assert.Equal(driverId, driverInstructions.First().DriverId);
        }
    }

}
