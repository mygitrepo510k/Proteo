using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories;
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

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }


        [Fact]
        public async Task AuthenticationService_DriverList_Empty()
        {
            base.ClearAll();

            // driver list is empty
            _fixture.Inject<IDriverRepository>(Mock.Of<IDriverRepository>(dr => dr.GetAll() == new List<Driver>()));

            var service = _fixture.Create<AuthenticationService>();
            var result = await service.AuthenticateAsync("9999");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task AuthenticationService_NoMatchingPassCodes()
        {
            base.ClearAll();

            // driver list contains non-matching passcodes
            _fixture.Inject<IDriverRepository>(Mock.Of<IDriverRepository>(dr => dr.GetAll() == _fixture.CreateMany<Driver>()));

            var service = _fixture.Create<AuthenticationService>();
            var result = await service.AuthenticateAsync("9999");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task AuthenticationService_MatchingPassCode()
        {
            base.ClearAll();

            // driver list contains a driver with a matching passcode
            Driver driver = new Driver() { Passcode = "9999" };
            _fixture.Inject<IDriverRepository>(Mock.Of<IDriverRepository>(dr => dr.GetAll() == new List<Driver>() { driver } ));

            var service = _fixture.Create<AuthenticationService>();
            var result = await service.AuthenticateAsync("9999");

            Assert.True(result.Success);
            Assert.Same(driver, result.Driver);
        }

    }

}
