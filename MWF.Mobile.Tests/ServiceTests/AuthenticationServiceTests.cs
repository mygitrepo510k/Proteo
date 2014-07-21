using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Tests.Helpers;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class AuthenticationServiceTests
        : MvxIoCSupportingTest
    {

        private IFixture _fixture;
        private IEnumerable<Driver> _matchingPasscodes;
        private IEnumerable<Driver> _noMatchingPasscodes;
        private Mock<IGatewayService> _gatewayServiceMock;


        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            Driver driver = new Driver() { Passcode = "9999" };
            _matchingPasscodes = new List<Driver>() { driver };
            _noMatchingPasscodes = _fixture.CreateMany<Driver>();

            //mock the gateway service
            _gatewayServiceMock = new Mock<IGatewayService>();
            _gatewayServiceMock.Setup(gs => gs.GetDrivers()).Returns(Task.FromResult<IEnumerable<Driver>>(new List<Driver>()));
            _fixture.Inject<IGatewayService>(_gatewayServiceMock.Object);

        }



        [Theory]
        [InlineData("matching", false, true, "matching", true)]             // Passcode exists in local database, shouldn't connect to Bluesphere
        [InlineData("not_matching", false, false, "matching", false)]       // Passcode doesn't exist in local database, but can't connect to bluesphere
        [InlineData("not_matching", true, true, "matching", true)]          // Passcode doesn't exist in local database, but does in Bluesphere. Can successfully connect to bluesphere.        
        [InlineData("not_matching", true, true, "not_matching", false)]     // Passcode doesn't exist in local database, doesn't exist in Bluesphere either.
        public async Task AuthenticationService_Authenticate(string driversFromDB, bool shouldUpdateFromBluesphere, bool isConnected, string driversFromBluesphere, bool expectedResult)
        {
            base.ClearAll();

            IEnumerable<Driver> drivers = GetDrivers(driversFromDB);

            //mock the reachability service
            _fixture.Inject<IReachability>(Mock.Of<IReachability>(r => r.IsConnected() == isConnected));

            // mock driver repository. Returns driversFromDB first time when asked, and driversFromBluesphere subsequently
            Mock<IDriverRepository> driverRepositoryMock = new Mock<IDriverRepository>();
            driverRepositoryMock.Setup(dr => dr.GetAll()).ReturnsInOrder(GetDrivers(driversFromDB), GetDrivers(driversFromBluesphere));
            _fixture.Inject<IDriverRepository>(driverRepositoryMock.Object);

            var service = _fixture.Create<AuthenticationService>();
            var result = await service.AuthenticateAsync("9999");

            Assert.Equal(expectedResult, result.Success);
            if (expectedResult) Assert.Equal(_matchingPasscodes.First(), result.Driver);
            else Assert.NotEmpty(result.AuthenticationFailedMessage);

            //verify that if bluesphere should have been connected to, it has done so
            if (shouldUpdateFromBluesphere)
            {
                _gatewayServiceMock.Verify(gs => gs.GetDrivers(), Times.Once);
                //driver repostory should have been updated to as part of bluesphere refresh
                driverRepositoryMock.Verify(dr => dr.DeleteAll(), Times.Once);
                driverRepositoryMock.Verify(dr => dr.Insert(It.IsAny<IEnumerable<Driver>>()), Times.Once);
            }

        }


        #region Helper Methods

        private IEnumerable<Driver> GetDrivers(string driverSpecification)
        {
            if (driverSpecification=="matching")
            {
                return _matchingPasscodes;
            }
            else
            {
                return _noMatchingPasscodes;
            }

        }

        #endregion



    }

}
