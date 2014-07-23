﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class VehicleListViewModelTests 
        : MvxIoCSupportingTest
    {

        private IFixture _fixture;
        private Driver _driver;
        private Vehicle _vehicle;
        private IStartupInfoService _startupInfoServeice;
        private Mock<ICurrentDriverRepository> _currentDriverRepository;

        protected override void AdditionalSetup()
        {
            var mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(mockDispatcher);

            var mockUserInteraction = new Mock<IUserInteraction>();
            mockUserInteraction.Setup(ui => ui.Confirm(It.IsAny<string>(), It.IsAny<Action<bool>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));
            Ioc.RegisterSingleton<IUserInteraction>(mockUserInteraction.Object);


            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _driver = new Core.Models.Driver() { LastName = "TestName", ID = new Guid()  };

            _vehicle = new Core.Models.Vehicle() { Registration = "TestRegistration", ID = new Guid() };

            _startupInfoServeice = new StartupInfoService();
            _startupInfoServeice.LoggedInDriver = _driver;
            _fixture.Inject<IStartupInfoService>(_startupInfoServeice);

            _currentDriverRepository = new Mock<ICurrentDriverRepository>();
            _currentDriverRepository.Setup(cdr => cdr.GetByID(It.IsAny<Guid>())).Returns(new CurrentDriver());
            _fixture.Inject<ICurrentDriverRepository>(_currentDriverRepository.Object);

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(new AuthenticationResult { Success = false });
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.Is<string>(s => s == "9999"))).ReturnsAsync(new AuthenticationResult { Success = true, Driver = _driver });
            _fixture.Inject<IAuthenticationService>(mockAuthenticationService.Object);

        }

        /// <summary>
        /// Tests that on successful authentication the TrailerListViewModel is navigated to
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulAuthenticationRedirectsToTrailerListView()
        {
            base.ClearAll();

            var vm = _fixture.Create<VehicleListViewModel>();


            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            //Assert.Equal(1, mockDispatcher.Requests.Count);
            var request = mockDispatcher.Requests.First();
            Assert.Equal(typeof(TrailerListViewModel), request.ViewModelType);

        }

        /// <summary>
        /// Tests that on successful authentication a drivers vehicle ID 
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulAuthenticationStoresDriverVehicleID()
        {
            base.ClearAll();

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            Assert.NotNull(_startupInfoServeice.LoggedInDriver);
            Assert.Equal(_vehicle.ID, _startupInfoServeice.LoggedInDriver.LastVehicleID);

        }

        /// <summary>
        /// Tests that on successful authentication a current drivers vehicle ID 
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulAuthenticationStoresCurrentDriverVehicleID()
        {
            base.ClearAll();

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            _currentDriverRepository.Verify(cdr => cdr.Insert(It.Is<CurrentDriver>(cd => cd.ID == _driver.LastVehicleID)), Times.Once);   
        }

        /// <summary>
        /// Tests that the toast message appears when there is no internet connection
        /// on the refresh of the vehicle list.
        /// </summary>
        [Fact]
        public void VehicleListVM_NoInternetShowToast()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == false));

            var toast = new Mock<IToast>();
            _fixture.Inject<IToast>(toast.Object);

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.RefreshListCommand.Execute(null);

            toast.Verify(t => t.Show("No internet connection!"));
             
        }
    }
}
