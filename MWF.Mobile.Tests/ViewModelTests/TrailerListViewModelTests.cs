using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
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
    public class TrailerListViewModelTests
        :  MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Driver _driver;
        private Trailer _trailer;
        private TrailerItemViewModel _trailerItemViewModel;
        private IInfoService _infoService;
        private Mock<ICurrentDriverRepository> _currentDriverRepository;
        private Mock<ICustomUserInteraction> _mockUserInteraction;

        protected override void AdditionalSetup()
        {
            var mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(mockDispatcher);

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customize<TrailerListViewModel>(tlvm => tlvm.Without(x => x.DefaultTrailerReg));

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _driver = new Core.Models.Driver() { LastName = "TestName", ID = new Guid() };

            _trailer = new Core.Models.Trailer() { Registration = "TestRegistration", ID = Guid.NewGuid() };
            _trailerItemViewModel = new TrailerItemViewModel() { Trailer = _trailer };

            _infoService = _fixture.Create<InfoService>();
            _infoService.LoggedInDriver = _driver;
            _fixture.Inject<IInfoService>(_infoService);

            _currentDriverRepository = new Mock<ICurrentDriverRepository>();
            _currentDriverRepository.Setup(cdr => cdr.GetByID(It.IsAny<Guid>())).Returns(new CurrentDriver());
            _fixture.Inject<ICurrentDriverRepository>(_currentDriverRepository.Object);

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(new AuthenticationResult { Success = false });
            mockAuthenticationService.Setup(m => m.AuthenticateAsync(It.Is<string>(s => s == "9999"))).ReturnsAsync(new AuthenticationResult { Success = true, Driver = _driver });
            _fixture.Inject<IAuthenticationService>(mockAuthenticationService.Object);
        }

        /// <summary>
        /// Tests that on successful authentication the SafetyCheckViewModel is navigated to
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulAuthenticationRedirectsToSafetyCheckListView()
        {
            base.ClearAll();

            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(ns => ns.MoveToNext());
            _fixture.Inject<INavigationService>(navigationServiceMock.Object);

            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Confirm your trailer");

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.TrailerSelectCommand.Execute(_trailerItemViewModel);

            // check that the navigation service was called
            navigationServiceMock.Verify(ns => ns.MoveToNext(), Times.Once);

        }

        /// <summary>
        /// Tests that on successful authentication a drivers trailer ID 
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulAuthenticationStoresDriverTrailerID()
        {
            base.ClearAll();

            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Confirm your trailer");

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.TrailerSelectCommand.Execute(_trailerItemViewModel);

            Assert.NotNull(_infoService.LoggedInDriver);
            Assert.Equal(_trailer.ID, _infoService.LoggedInDriver.LastSecondaryVehicleID);

        }

        /// <summary>
        /// Tests that on successful selection of no trailer 
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulSelectionOfNoTrailer()
        {
            base.ClearAll();

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.NoTrailerSelectCommand.Execute(null);

            Assert.NotNull(_infoService.LoggedInDriver);
            Assert.Equal(Guid.Empty, _infoService.LoggedInDriver.LastSecondaryVehicleID);

        }

        /// <summary>
        /// Tests that the toast message appears when there is no internet connection
        /// on the refresh of the trailer list.
        /// </summary>
        [Fact(Skip = "Temporarily disabled during trailer select refactor")]
        public void TrailerListVM_NoInternetShowToast()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == false));

            var toast = new Mock<IToast>();
            _fixture.Inject<IToast>(toast.Object);

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.RefreshListCommand.Execute(null);

            toast.Verify(t => t.Show("No internet connection!"));

        }

        /// <summary>
        /// Tests that the list is filtered correctly when you search for a trailer.
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulTrailerListFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var trailerRepository = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>();
            trailerRepository.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.TrailerSearchText = trailers.First().Registration;

            Assert.Equal(1, vm.Trailers.ToList().Count);

            Assert.Equal(trailers.First(), vm.Trailers.First().Trailer);

        }

        /// <summary>
        /// Tests that the refresh function on the trailer list works.
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulTrailerListRefreshNoFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var trailerRepository = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>();
            trailerRepository.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.TrailerSearchText = "";

            vm.RefreshListCommand.Execute(null);

            var trailerModels = vm.Trailers.Select(x => x.Trailer);

            Assert.Equal(trailers, trailerModels);
            //Its get all twice because it calls it once on setup and another on refresh
            trailerRepository.Verify(vr => vr.GetAll(), Times.Exactly(2));

        }

        /// <summary>
        /// Tests that the refresh function on the trailer list works but refilters the list.
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulTrailerListRefreshFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var trailerRepository = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>(20);
            trailerRepository.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.TrailerSearchText = "Registration";

            vm.RefreshListCommand.Execute(null);

            var trailerModels = vm.Trailers.Select(x => x.Trailer);

            Assert.Equal(trailers, trailerModels);
            //Its get all twice because it calls it once on setup and another on refresh
            trailerRepository.Verify(vr => vr.GetAll(), Times.Exactly(2));

        }
    }
}
