using Chance.MvvmCross.Plugins.UserInteraction;
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
    public class TrailerListViewModelTests
        :  MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Driver _driver;
        private Trailer _trailer;
        private IStartupService _startupService;
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

            _driver = new Core.Models.Driver() { LastName = "TestName", ID = new Guid() };

            _trailer = new Core.Models.Trailer() { Registration = "TestRegistration", ID = Guid.NewGuid() };

            _startupService = _fixture.Create<StartupService>();
            _startupService.LoggedInDriver = _driver;
            _fixture.Inject<IStartupService>(_startupService);

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

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.TrailerSelectorCommand.Execute(_trailer);

            var mockDispatcher = Ioc.Resolve<IMvxMainThreadDispatcher>() as MockDispatcher;
            Assert.Equal(1, mockDispatcher.Requests.Count);
            var request = mockDispatcher.Requests.First();
            Assert.Equal(typeof(SafetyCheckViewModel), request.ViewModelType);

        }

        /// <summary>
        /// Tests that on successful authentication a drivers trailer ID 
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulAuthenticationStoresDriverTrailerID()
        {
            base.ClearAll();

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.TrailerSelectorCommand.Execute(_trailer);

            Assert.NotNull(_startupService.LoggedInDriver);
            Assert.Equal(_trailer.ID, _startupService.LoggedInDriver.LastSecondaryVehicleID);

        }

        /// <summary>
        /// Tests that on successful selection of no trailer 
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulSelectionOfNoTrailer()
        {
            base.ClearAll();

            var vm = _fixture.Create<TrailerListViewModel>();

            vm.NoTrailerSelectorCommand.Execute(null);

            Assert.NotNull(_startupService.LoggedInDriver);
            Assert.Equal(Guid.Empty, _startupService.LoggedInDriver.LastSecondaryVehicleID);

        }

        /// <summary>
        /// Tests that the toast message appears when there is no internet connection
        /// on the refresh of the trailer list.
        /// </summary>
        [Fact]
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

            var trailerRepositry = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>();
            trailerRepositry.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.SearchText = "registration";

            Assert.Equal(trailers, vm.Trailers);

        }

        /// <summary>
        /// Tests that the refresh function on the trailer list works.
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulTrailerListRefreshNoFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var trailerRepositry = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>();
            trailerRepositry.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.SearchText = "";

            vm.RefreshListCommand.Execute(null);

            Assert.Equal(trailers, vm.Trailers);
            //Its get all twice because it calls it once on setup and another on refresh
            trailerRepositry.Verify(vr => vr.GetAll(), Times.Exactly(2));

        }

        /// <summary>
        /// Tests that the refresh function on the trailer list works but refilters the list.
        /// </summary>
        [Fact]
        public void TrailerListVM_SuccessfulTrailerListRefreshFilter()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            var trailerRepositry = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Trailer>(20);
            trailerRepositry.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepositry.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<TrailerListViewModel>();
            vm.SearchText = "Registration";

            vm.RefreshListCommand.Execute(null);

            Assert.Equal(trailers, vm.Trailers);
            //Its get all twice because it calls it once on setup and another on refresh
            trailerRepositry.Verify(vr => vr.GetAll(), Times.Exactly(2));

        }
    }
}
