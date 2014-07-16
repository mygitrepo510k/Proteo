﻿using System;
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

namespace MWF.Mobile.Tests.ViewModelTests
{
    
    public class PasscodeViewModelTests
        : MvxIoCSupportingTest
    {

        protected override void AdditionalSetup()
        {
            var mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(mockDispatcher);

            var mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(mockUserInteraction.Object);

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(new AuthenticationResult { Success = false });
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.Is<string>(s => s == "9999"))).ReturnsAsync(new AuthenticationResult { Success = true });
            Ioc.RegisterSingleton<IAuthenticationService>(mockAuthenticationService.Object);
        }

        /// <summary>
        /// Tests that on successful authentication the VehicleListViewModel is navigated to
        /// </summary>
        [Fact]
        public void PasscodeVM_SuccessfulAuthenticationRedirectsToVehicleView()
        {
            base.ClearAll();

            var mockAuthenticationService = Ioc.Resolve<IAuthenticationService>();
            var vm = new PasscodeViewModel(mockAuthenticationService) { Passcode = "9999" };

            vm.LoginCommand.Execute(null);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(1, mockDispatcher.Requests.Count);
            var request = mockDispatcher.Requests.First();
            Assert.Equal(typeof(VehicleListViewModel), request.ViewModelType);
        }


        [Fact]
        public void PasscodeVM_BlankPasscodeDoesntRedirectToVehicleView()
        {
            base.ClearAll();

            var mockAuthenticationService = Ioc.Resolve<IAuthenticationService>();
            var vm = new PasscodeViewModel(mockAuthenticationService) { Passcode = "" };

            vm.LoginCommand.Execute(null);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(0, mockDispatcher.Requests.Count);
        }


        [Fact]
        public void PasscodeVM_IncorrectPassword()
        {
            base.ClearAll();

            // Set incorrect password
            var mockAuthenticationService = Ioc.Resolve<IAuthenticationService>();
            var vm = new PasscodeViewModel(mockAuthenticationService) { Passcode = "1212" };

            vm.LoginCommand.Execute(null);

            // Check we didn't redirect anywhere
            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(0, mockDispatcher.Requests.Count);

            //Check that the passcode got blanked out
            Assert.Equal(string.Empty, vm.Passcode);
        }

    }

}
