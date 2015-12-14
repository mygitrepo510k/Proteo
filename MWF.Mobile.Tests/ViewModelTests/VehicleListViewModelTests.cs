using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class VehicleListViewModelTests 
        : MvxIoCSupportingTest
    {

        private IFixture _fixture;
        private Driver _driver;
        private Vehicle _vehicle;
        private IInfoService _infoService;
        private Mock<ICurrentDriverRepository> _currentDriverRepository;
        private Mock<ICustomUserInteraction> _mockUserInteraction;

        protected override void AdditionalSetup()
        {
            _mockUserInteraction = new Mock<ICustomUserInteraction>();
            _mockUserInteraction.ConfirmReturnsFalseIfTitleStartsWith("Last Used Vehicle");
            Ioc.RegisterSingleton<ICustomUserInteraction>(_mockUserInteraction.Object);

            _mockUserInteraction = new Mock<ICustomUserInteraction>();
            
            Ioc.RegisterSingleton<ICustomUserInteraction>(_mockUserInteraction.Object);


            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _driver = new Core.Models.Driver() { LastName = "TestName", ID = new Guid()  };

            _vehicle = new Core.Models.Vehicle() { Registration = "TestRegistration", ID = new Guid() };

            _infoService = _fixture.Create<InfoService>();
            _infoService.LoggedInDriver = _driver;
            _fixture.Inject<IInfoService>(_infoService);

            _currentDriverRepository = new Mock<ICurrentDriverRepository>();
            _currentDriverRepository.Setup(cdr => cdr.GetByIDAsync(It.IsAny<Guid>())).ReturnsAsync(new CurrentDriver());
            _fixture.Inject<ICurrentDriverRepository>(_currentDriverRepository.Object);

        }

        /// <summary>
        /// Tests that when a vehicle is selected and the confirm message is shown then the navigation service is called 
        /// </summary>
        [Fact]
        public void VehicleListVM_SelectVehicle_Navigation()
        {
            base.ClearAll();

            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Confirm your vehicle");

            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(ns => ns.MoveToNextAsync());
            _fixture.Inject<INavigationService>(navigationServiceMock.Object);

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            // check that the navigation service was called
            navigationServiceMock.Verify(ns => ns.MoveToNextAsync(), Times.Once);

        }

        /// <summary>
        /// Tests that vehicle is selected and the confirm message the vehicle is stored
        /// </summary>
        [Fact]
        public void VehicleListVM_SelectVehicle_VehicleStored()
        {
            base.ClearAll();

            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Confirm your vehicle");

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            Assert.NotNull(_infoService.LoggedInDriver);
            Assert.Equal(_vehicle.ID, _infoService.LoggedInDriver.LastVehicleID);

        }

        /// <summary>
        /// Tests that on successful authentication a current drivers vehicle ID 
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulAuthenticationStoresCurrentDriverVehicleID()
        {
            base.ClearAll();

            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Confirm your vehicle");

            var vm = _fixture.Create<VehicleListViewModel>();

            vm.ShowVehicleDetailCommand.Execute(_vehicle);

            _currentDriverRepository.Verify(cdr => cdr.InsertAsync(It.Is<CurrentDriver>(cd => cd.ID == _driver.LastVehicleID)), Times.Once);   
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

        /// <summary>
        /// Tests that the list is filtered correctly when you search for a vehicle.
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulVehicleListFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var vehicleRepositry = new Mock<IVehicleRepository>();
            var vehicles = _fixture.CreateMany<Vehicle>();
            vehicleRepositry.Setup(vr => vr.GetAllAsync()).ReturnsAsync(vehicles);

            _fixture.Inject<IVehicleRepository>(vehicleRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<VehicleListViewModel>();
            vm.VehicleSearchText = "registration";

            Assert.Equal(vehicles, vm.Vehicles);

        }

        /// <summary>
        /// Tests that the refresh function on the vehicle list works.
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulVehicleListRefreshNoFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var vehicleRepositry = new Mock<IVehicleRepository>();
            var vehicles = _fixture.CreateMany<Vehicle>();
            vehicleRepositry.Setup(vr => vr.GetAllAsync()).ReturnsAsync(vehicles);

            _fixture.Inject<IVehicleRepository>(vehicleRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<VehicleListViewModel>();
            vm.VehicleSearchText = "";

            vm.RefreshListCommand.Execute(null);

            Assert.Equal(vehicles, vm.Vehicles);
            //Its get all twice because it calls it once on setup and another on refresh
            vehicleRepositry.Verify(vr => vr.GetAllAsync(), Times.Exactly(2));
  
        }
        
        /// <summary>
        /// Tests that the refresh function on the vehicle list works but refilters the list.
        /// </summary>
        [Fact]
        public void VehicleListVM_SuccessfulVehicleListRefreshFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var vehicleRepositry = new Mock<IVehicleRepository>();
            var vehicles = _fixture.CreateMany<Vehicle>(20);
            vehicleRepositry.Setup(vr => vr.GetAllAsync()).ReturnsAsync(vehicles);

            _fixture.Inject<IVehicleRepository>(vehicleRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<VehicleListViewModel>();
            vm.VehicleSearchText = "Registration";

            vm.RefreshListCommand.Execute(null);

            Assert.Equal(vehicles, vm.Vehicles);
            //Its get all twice because it calls it once on setup and another on refresh
            vehicleRepositry.Verify(vr => vr.GetAllAsync(), Times.Exactly(2));

        }
         
    }
}
