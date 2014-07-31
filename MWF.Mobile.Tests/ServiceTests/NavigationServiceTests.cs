using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Presentation;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Chance.MvvmCross.Plugins.UserInteraction;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class NavigationServiceTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MockDispatcher _mockViewDispatcher;
        private Mock<IUserInteraction> _mockUserInteraction;
           

        protected override void AdditionalSetup()
        {

            _mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(_mockUserInteraction.Object);

            _mockViewDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(_mockViewDispatcher);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

        }

        #endregion

        #region Core Service Tests

        [Fact]
        public void NavigationService_InsertNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            Assert.True(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

        }

        [Fact]
        public void NavigationService_InsertCustomNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            bool testBool = false;

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertCustomNavAction<ActivityViewModel, FragmentViewModel1>( () => testBool = true );

            Assert.True(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            navAction.Invoke();

            Assert.True(testBool);

        }

        [Fact]
        public void NavigationService_InsertBackNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/PassCode view model we should navigate back to customer code model
            service.InsertBackNavAction<ActivityViewModel, FragmentViewModel2>(typeof(FragmentViewModel1));

            Assert.True(service.BackNavActionExists<ActivityViewModel, FragmentViewModel2>());

        }


        [Fact]
        public void NavigationService_InsertCustomBackNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            bool testBool = false;

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertCustomBackNavAction<ActivityViewModel, FragmentViewModel1>(() => testBool = true);

            Assert.True(service.BackNavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

            navAction.Invoke();

            Assert.True(testBool);

        }


        [Fact]
        public void NavigationService_NavActionExists_NoNavActionDefined()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Assert.False(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

        }

        [Fact]
        public void NavigationService_GetNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            Action navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_GetBackNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/APsscode view model we should navigate to FragmentViewModel1
            service.InsertBackNavAction<ActivityViewModel, FragmentViewModel2>(typeof(FragmentViewModel1));

            Action navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel2));

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke();

            //Check that the customer view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Hints.Count);
            var hint = _mockViewDispatcher.Hints.First();
            Assert.Equal(typeof(FragmentViewModel1), (hint as CloseUpToViewPresentationHint).ViewModelType);

        }


        [Fact]
        public void NavigationService_GetNavAction_DynamicOverload()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            Action navAction = service.GetNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_GetNavAction_NoNavActionDefined()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Action navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            Assert.Null(navAction);

        }

        [Fact]
        public void NavigationService_MoveToNext()
        {
            base.ClearAll();

            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel1>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            // Move to the next view model
            service.MoveToNext();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_GoBack()
        {
            base.ClearAll();
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => 
                                                                cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel2>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Specify that from current view models we should navigate back to FragmentViewModel1
            service.InsertBackNavAction<ActivityViewModel, FragmentViewModel2>(typeof(FragmentViewModel1));

            // Move to the next view model
            Assert.True(service.IsBackActionDefined());

            service.GoBack();

            //Check that the customer view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Hints.Count);
            var hint = _mockViewDispatcher.Hints.First();
            Assert.Equal(typeof(FragmentViewModel1), (hint as CloseUpToViewPresentationHint).ViewModelType);

        }

        #endregion

        #region Mapping Tests (test that the service has the mappings to correctly navigate from viewmodel to viewmodel corectly)


        [Fact]
        public void NavigationService_Mappings_CustomerCode()
        {
            base.ClearAll();

            // presenter will report the current activity view model as a StartUpViewModel, current fragment model a customer model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CustomerCodeViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

 
            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(PasscodeViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_Passcode()
        {
            base.ClearAll();

            // presenter will report the current activity view model as a StartUpViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<PasscodeViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the safetycheck view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(VehicleListViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_VehicleList()
        {
            base.ClearAll();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a vehicle list view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<VehicleListViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(TrailerListViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_TrailerList()
        {
            base.ClearAll();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a vehicle list view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<TrailerListViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the safetycheck view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(SafetyCheckViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_SafetyCheck_OdometerRequired()
        {
            base.ClearAll();

            //omit properties causing circular dependencies
            var safetyCheckViewModel = _fixture.Build<SafetyCheckViewModel>().Without(s=>s.SafetyCheckItemViewModels).Create<SafetyCheckViewModel>();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a safety check view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == safetyCheckViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            SetUpOdometerRequired(true);

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the odometer view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(OdometerViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_SafetyCheck_OdometerNotRequired()
        {
            base.ClearAll();

            //omit properties causing circular dependencies
            var safetyCheckViewModel = _fixture.Build<SafetyCheckViewModel>().Without(s => s.SafetyCheckItemViewModels).Create<SafetyCheckViewModel>();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a safety check view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == safetyCheckViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            SetUpOdometerRequired(false);

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the signature screen view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(SafetyCheckSignatureViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_SafetyCheck_NoProfiles()
        {
            base.ClearAll();

            //omit properties causing circular dependencies
            var safetyCheckViewModel = _fixture.Build<SafetyCheckViewModel>().Without(s => s.SafetyCheckItemViewModels).Create<SafetyCheckViewModel>();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a safety check view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == safetyCheckViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            // Set up so that there are no safety profiles
            SetUpNoSafetyProfiles();

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the main activity view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(MainViewModel), request.ViewModelType);

        }


        [Fact]
        public void NavigationService_Mappings_Odometer()
        {
            base.ClearAll();

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as an odometer view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OdometerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(SafetyCheckSignatureViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_Signature_NoFaults()
        {
            base.ClearAll();

            // Set up so that the safety check data has no faults
            SetUpSafetyCheckData(false);

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a safety check view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SafetyCheckSignatureViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the main activity view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(MainViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_Signature_Faults()
        {
            base.ClearAll();

            // Set up so that the safety check data has faults
            SetUpSafetyCheckData(true);

            // presenter will report the current activity view model as StartUpViewModel, current fragment model as a safety check view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SafetyCheckSignatureViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);



            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext();

            //Check that the main activity view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Hints.Count);
            var hint = _mockViewDispatcher.Hints.First();
            Assert.Equal(typeof(PasscodeViewModel), (hint as CloseUpToViewPresentationHint).ViewModelType);

        }

        [Fact]
        public void NavigationService_BackMappings_Passcode()
        {
            base.ClearAll();

            var closeApplicationMock = new Mock<ICloseApplication>();
            _fixture.Inject<ICloseApplication>(closeApplicationMock.Object);

            // presenter will report the current activity view model as a StartUpViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<PasscodeViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.GoBack();

            //Check that the application was closed
            closeApplicationMock.Verify(ca => ca.CloseApp(), Times.Once);
        

        }


        #endregion


        #region Helper Functions

        private void SetUpOdometerRequired(bool required)
        {
            //create vehicle and a safety profile that link together in repositories
            var vehicle = _fixture.Create<Vehicle>();
            var safetyProfile = _fixture.Create<SafetyProfile>();
            safetyProfile.IntLink = vehicle.SafetyCheckProfileIntLink;
            safetyProfile.OdometerRequired = required;

            var safetyProfileRepositoryMock = new Mock<ISafetyProfileRepository>();
            safetyProfileRepositoryMock.Setup(s => s.GetAll()).Returns(new List<SafetyProfile>() {safetyProfile}) ;
            _fixture.Inject<ISafetyProfileRepository>(safetyProfileRepositoryMock.Object);

            var startupService = Mock.Of<IStartupService>(s => 
                                                          s.LoggedInDriver.LastVehicleID == vehicle.ID &&
                                                          s.CurrentVehicle == vehicle);
            _fixture.Inject<IStartupService>(startupService);


            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

        }

        private void SetUpNoSafetyProfiles()
        {
            //create vehicle and a safety profile that link together in repositories
            var vehicle = _fixture.Create<Vehicle>();
            var safetyProfile = _fixture.Create<SafetyProfile>();
            safetyProfile.IntLink = vehicle.SafetyCheckProfileIntLink;
            safetyProfile.OdometerRequired = true;

            //No safety profiles
            var safetyProfileRepositoryMock = new Mock<ISafetyProfileRepository>();
            safetyProfileRepositoryMock.Setup(s => s.GetAll()).Returns(new List<SafetyProfile>());
            _fixture.Inject<ISafetyProfileRepository>(safetyProfileRepositoryMock.Object);

            var startupService = Mock.Of<IStartupService>(s => s.LoggedInDriver.LastVehicleID == vehicle.ID &&
                                                          s.CurrentVehicle == vehicle);
            _fixture.Inject<IStartupService>(startupService);


            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

        }

        private void SetUpSafetyCheckData(bool faults)
        {
            // Create safety check
            var latestSafetyCheck = _fixture.Create<LatestSafetyCheck>();
            IEnumerable<SafetyCheckData> safetyCheckData = new List<SafetyCheckData>() { latestSafetyCheck.VehicleSafetyCheck };

            if (!faults)
            {
                foreach (var fault in latestSafetyCheck.VehicleSafetyCheck.Faults)
                {
                    fault.Status = Core.Enums.SafetyCheckStatus.Passed;
                }
                
            }
            else
            {
               latestSafetyCheck.VehicleSafetyCheck.Faults[0].Status = Core.Enums.SafetyCheckStatus.Failed;               
            }

            var startupService = Mock.Of<IStartupService>(s => 
                                                          s.LoggedInDriver == _fixture.Create<Driver>() &&
                                                          s.CurrentVehicle == _fixture.Create<Vehicle>() &&
                                                          s.GetCurrentSafetyCheckData() == safetyCheckData  );

            _fixture.Inject<IStartupService>(startupService);

        }

        #endregion

    }


    #region Test Classes

    public class ActivityViewModel : BaseActivityViewModel
    {

    }

    public class FragmentViewModel1 : BaseFragmentViewModel
    {
        public override string FragmentTitle { get { return "FragmentViewModel1"; } }
    }

    public class FragmentViewModel2 : BaseFragmentViewModel
    {
        public override string FragmentTitle { get { return "FragmentViewModel2"; } }
    }

    #endregion
}
