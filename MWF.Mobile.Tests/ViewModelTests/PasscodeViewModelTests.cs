using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using Xunit;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace MWF.Mobile.Tests.ViewModelTests
{
    
    public class PasscodeViewModelTests
        : MvxIoCSupportingTest
    {

        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            var mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(mockDispatcher);

            var mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(mockUserInteraction.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(new AuthenticationResult { Success = false });
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.Is<string>(s => s == "9999"))).ReturnsAsync(new AuthenticationResult { Success = true, Driver = new Core.Models.Driver() { LastName = "TestName" } });
            _fixture.Inject<IAuthenticationService>(mockAuthenticationService.Object);
        }

        /// <summary>
        /// Tests that on successful authentication the VehicleListViewModel is navigated to
        /// </summary>
        [Fact]
        public void PasscodeVM_SuccessfulAuthenticationRedirectsToVehicleListView()
        {
            base.ClearAll();

            var vm = _fixture.Create<PasscodeViewModel>();
            vm.Passcode = "9999";

            vm.LoginCommand.Execute(null);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(1, mockDispatcher.Requests.Count);
            var request = mockDispatcher.Requests.First();
            Assert.Equal(typeof(VehicleListViewModel), request.ViewModelType);

        }

        /// <summary>
        /// Tests that on successful authentication a driver 
        /// </summary>
        [Fact]
        public void PasscodeVM_SuccessfulAuthenticationStoresDriver()
        {
            base.ClearAll();

            var startUpInfoService = new StartupInfoService();
            _fixture.Inject<IStartupInfoService>(startUpInfoService);
            var vm = _fixture.Create<PasscodeViewModel>();
            vm.Passcode = "9999";

            vm.LoginCommand.Execute(null);

            Assert.NotNull(startUpInfoService.LoggedInDriver);
            Assert.Equal("TestName", startUpInfoService.LoggedInDriver.LastName);

        }


        [Fact]
        public void PasscodeVM_BlankPasscodeDoesntRedirectToVehicleView()
        {
            base.ClearAll();

            var vm = _fixture.Create<PasscodeViewModel>();
            vm.Passcode = "";

            vm.LoginCommand.Execute(null);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(0, mockDispatcher.Requests.Count);
        }


        [Fact]
        public void PasscodeVM_IncorrectPassword()
        {
            base.ClearAll();

            // Set incorrect password
            var vm = _fixture.Create<PasscodeViewModel>();
            vm.Passcode = "1212";
        

            vm.LoginCommand.Execute(null);

            // Check we didn't redirect anywhere
            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(0, mockDispatcher.Requests.Count);

            //Check that the passcode got blanked out
            Assert.Equal(string.Empty, vm.Passcode);
        }

    }

}
