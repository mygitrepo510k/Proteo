using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Presentation;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Chance.MvvmCross.Plugins.UserInteraction;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;
using MWF.Mobile.Tests.Helpers;
using MWF.Mobile.Core.Repositories.Interfaces;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class NavigationServiceTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MockDispatcher _mockViewDispatcher;
        private Mock<IUserInteraction> _mockUserInteraction;
        private Mock<IMvxMessenger> _mockMessenger;

        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<IApplicationProfileRepository> _mockApplicationProfile;
        private Mock<IDataChunkService> _mockDataChunkService;
        private Mock<ISafetyProfileRepository> _mockSafetyProfileRepository;

           

        protected override void AdditionalSetup()
        {

            _mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Complete Instruction");
            _mockUserInteraction.Setup(mui => mui.ConfirmAsync(It.IsAny<string>(), It.Is<string>(s => s == "Change Trailer?"), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync<IUserInteraction,bool>(true);
            _mockUserInteraction.Setup(mui => mui.ConfirmAsync(It.Is<string>(s => s == "Do you want to enter a comment for this instruction?"),It.IsAny<string>() , It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync<IUserInteraction,bool>(true);

            Ioc.RegisterSingleton<IMvxStringToTypeParser>(new MvxStringToTypeParser());

            _mockViewDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(_mockViewDispatcher);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mockMessenger = new Mock<IMvxMessenger>();
            Ioc.RegisterSingleton<IMvxMessenger>(_mockMessenger.Object);

            _mobileData = _fixture.Create<MobileData>();


            _mockApplicationProfile = _fixture.InjectNewMock<IApplicationProfileRepository>();
            _mockApplicationProfile.Setup(map => map.GetAll()).Returns(_fixture.CreateMany<ApplicationProfile>());

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _mockSafetyProfileRepository = _fixture.InjectNewMock<ISafetyProfileRepository>();

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockDataChunkService = _fixture.InjectNewMock<IDataChunkService>();

            var test = Ioc.Resolve<IMvxMessenger>();

        }

        #endregion

        #region Core Service Tests

        [Fact]
        public void NavigationService_InsertNavAction()
        {

            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            Assert.True(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

        }

        [Fact]
        public void NavigationService_InsertNavAction_InvalidDestType()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Destination type is not an MvxModel
            Assert.Throws<ArgumentException>(() => service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(HttpService)));

        }

        [Fact]
        public void NavigationService_InsertCustomNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Object myObj = null;
            Driver driver = _fixture.Create<Driver>();

            NavData<Driver> navData = new NavData<Driver> { Data = driver }; 

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertCustomNavAction<ActivityViewModel, FragmentViewModel1>( (a) =>  myObj = a);

            Assert.True(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action<NavData> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            navAction.Invoke(navData);

            Assert.Equal(navData, myObj);

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

            Object myObj = null;
            Driver driver = _fixture.Create<Driver>();

            NavData<Driver> navData = new NavData<Driver> { Data = driver }; 

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertCustomBackNavAction<ActivityViewModel, FragmentViewModel1>((a) => myObj = a);

            Assert.True(service.BackNavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action<NavData> navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

            navAction.Invoke(navData);

            Assert.Equal(navData, myObj);         

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

            Action<NavData> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke(null);

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

            Action<NavData> navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel2));

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke(null);

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

            Action<NavData> navAction = service.GetNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke(null);

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_GetNavAction_DynamicOverload_InvalidTypes()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // first class is not a BaseActivityViewModel
            Assert.Throws<ArgumentException>(() => service.GetNavAction(typeof(HttpService), typeof(FragmentViewModel1)));

            //second class is not a BaseFragmentViewModel
            Assert.Throws<ArgumentException>(() => service.GetNavAction(typeof(ActivityViewModel), typeof(HttpService)));

        }

        [Fact]
        public void NavigationService_GetNavAction_NoNavActionDefined()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Action<NavData> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

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
        public void NavigationService_MoveToNext_WithNavData()
        {
            base.ClearAll();

            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel1>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();
            Ioc.RegisterSingleton<INavigationService>(service);

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            //Create an object to pass through as a parameter 
            Driver driver = _fixture.Create<Driver>();
            Vehicle vehicle = _fixture.Create<Vehicle>();
            NavData<Driver> navData = new NavData<Driver> { Data = driver };
            navData.OtherData["vehicle"] = vehicle;

            service.MoveToNext(navData);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

            // check that the nav item guid was passed through correctly
            var parametersObjectOut = request.ParameterValues.First();
            Assert.Equal("NavGUID", parametersObjectOut.Key);
            Assert.Equal(navData.NavGUID.ToString(), parametersObjectOut.Value);

            // check that the nav item can be "re-inflated" by the nav service

            //clear down the nav data
            navData.OtherData = null;
            navData.Data = null;
            navData.Reinflate();

            Assert.Equal(driver, navData.Data);
            Assert.Equal(vehicle, navData.OtherData["vehicle"]);



        }

        [Fact]
        public void NavigationService_MoveToNext_UnknownMapping()
        {
            base.ClearAll();

            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel1>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Don't specify any mappings

            // Attempt to move to the next view model
            Assert.Throws<UnknownNavigationMappingException>(() => service.MoveToNext());


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

        [Fact]
        public void NavigationService_GoBack_UnknownMapping()
        {
            base.ClearAll();

            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel1>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Don't specify any mappings

            // Attempt to go back
            Assert.Throws<UnknownBackNavigationMappingException>(() => service.GoBack());


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
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(StartupViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_BackMappings_Passcode()
        {
            base.ClearAll();

            var closeApplicationMock = _fixture.InjectNewMock<ICloseApplication>();

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

        [Fact]
        public void NavigationService_BackMappings_Manifest()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.Collect;

            var manifestViewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            // presenter will report the current activity view model as a MainViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => 
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == manifestViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            // Go back
            service.GoBack();

            //Check that the startup activity view model was navigated to (passcode
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(StartupViewModel), request.ViewModelType);
        }



        [Fact]
        public void NavigationService_Mappings_Manifest_Instructions()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.Collect;

            var manifestViewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            // presenter will report the current activity view model as a MainViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == manifestViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavData<MobileData> navData = new NavData<MobileData> { Data = _mobileData };
            service.MoveToNext(navData);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);

            // check that the supplied parameters were passed through correctly
            var parametersObjectOut = request.ParameterValues.First();

            Assert.Equal("NavGUID", parametersObjectOut.Key);
            Assert.Equal(navData.NavGUID.ToString(), parametersObjectOut.Value);

        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Order()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            // nav item is an item (i.e. an order) indicating user has clicked on an order from the order list
            var navItemMock = Mock.Of<NavData<Item>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the order view  model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(OrderViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_Trailer()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            var navItem = new NavData<MWF.Mobile.Core.Models.Instruction.MobileData>();
            navItem.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            // Move to the next view model
            service.MoveToNext(navItem);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrailerViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavData<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_InstructionOnSiteToOrder()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            // nav item is an item (i.e. an order) indicating user has clicked on an order from the order list
            var navItemMock = Mock.Of<NavData<Item>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the order view  model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(OrderViewModel), request.ViewModelType);
        }



        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToCommentScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();       

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToCommentScreen_SkipClausedScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, false, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }


        #region Trailer Selection via "Change Trailer" Button on instruction screen

        // Tests the case when a user changes trailer via the "change trailer" button on the instruction screen and the trailer they select is the same
        // as the current trailer. Since no safety check logic is required they should be deposited directly back to the instruction screen,
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_TrailerToInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, InstructionProgress.NotStarted);


            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        // Tests the case when a user changes trailer via the "change trailer" button on the instruction screen and the trailer they select is the same
        // as the current trailer, but different from the one specified on the order. 
        // Since no safety check logic is required they should be deposited directly back to the instruction screen.
        // Addiotnally, the order should have been updated and the revised trailer data chunk fired
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_TrailerToInstruction_UpdateTrailerOnOrder()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, InstructionProgress.NotStarted);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer (but not the current order)
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;


            navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);


            // should have updated the trailer on the order
            Assert.Equal(trailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>( md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }

        // Tests the case when a user changes trailer via the "change trailer" button on the instruction screen and the trailer they select is the different
        // to the current trailer. Since a new trailer has been selected they should be direted to the safety check screen
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_TrailerToSafetyCheck()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, InstructionProgress.NotStarted);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same different to the current trailer
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSafetyCheckViewModel), request.ViewModelType);
        }


        // Tests the case on the safety check screen where no safety check profile was detected for the updated trailer, so no signature is required so the
        // user can be directed back to the instruction screen
        // Since the selected trailer differs from the one on the order then the order is updated and the revised trailer chunk set
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_SafetyCheck_ToInstruction_UpdateOrder()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;
            navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will be empty (so no safety profile can be retrieved for the trailer)
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>());

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no signature is required, the instruction screen is navigated back to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpService.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }


        // Tests that that when the instruction safety check signature screen is completed the user is directed back to the instruction screen
        // and the sfaety check data is comitted
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_SafetyCheckSignature_ToInstruction()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckSignatureViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();

            startUpServiceMock.SetupProperty(x => x.CurrentTrailer);
            startUpServiceMock.SetupProperty(x => x.CurrentTrailerSafetyCheckData);

            startUpServiceMock.Object.CurrentTrailer = _fixture.Create<Core.Models.Trailer>();

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            var updatedSafetyCheckData = navData.OtherData["UpdatedTrailerSafetyCheckData"] = _fixture.Create<SafetyCheckData>();
            navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will be empty (so no safety profile can be retrieved for the trailer)
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>());

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the instruction screen is navigated back to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));

            // should have set the current trailer
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer, trailer);
            
            // should have set the safety check data for the trailer
            Assert.Equal(updatedSafetyCheckData, startUpServiceMock.Object.CurrentTrailerSafetyCheckData);

            // check the safety check data was commited
            startUpServiceMock.Verify(ss => ss.CommitSafetyCheckData());
        }

        #endregion

        #region Collection On Site Trailer Select Flow Logic

        // Tests the case when the trailer confirmation setting is enabled and the user elects to select trailer
        // Should send the user to the trailer select screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToTrailerScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Change Trailer?");

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrailerViewModel), request.ViewModelType);
        }

        // Tests the case when the trailer confirmation setting is enabled and the user elects to use the current trailer
        // Should send the user to the trailer select screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSite_UseCurrentTrailer()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            mockUserInteraction.ConfirmAsyncReturnsFalseIfTitleStartsWith("Change Trailer?");

            // trailer prompt enabled, bypass comment, customer signatre/name required
            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, true, true, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            var navData = new NavData<MobileData>() { Data = mobileData };

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            // User elected to use the current trailer, so skip to the instruction signature
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();

            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(trailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }


        // Tests the case when the trailer confirmation setting is disabled (but the order and current trailer differ) and the user elects to select trailer
        // Should send the user to the trailer select screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSite_TrailerPromptDisabled_ToTrailerScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Change Trailer?");

            //trailer prompt disabled
            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, null);

            // current trailer will be different to that on the order
            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrailerViewModel), request.ViewModelType);
        }

        // Tests the case when the trailer confirmation setting is disabled (but the order and current trailer differ) and the user elects to use the current trailer
        // Should send the user to the trailer select screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSite_TrailerPromptDisabled_UseCurrentTrailer()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            mockUserInteraction.ConfirmAsyncReturnsFalseIfTitleStartsWith("Change Trailer?");

            // trailer prompt enabled, bypass comment, customer signatre/name required
            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, true, true, false, false, true, null);

            // current trailer will be different to that on the order
            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            // User elected to use the current trailer, so skip to the instruction signature
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();

            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpService.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }

        //Tests the case where a trailer was selected but it was the same as the current trailer
        //Since no safety check logic is required the user is moved directly onto the comment screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToCommentScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;
            mobileData.Order.Additional.Trailer.TrailerId = startUpService.CurrentTrailer.Registration;

            var service = _fixture.Create<NavigationService>();
           
            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no safety check logic was required, the comment view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        //Tests the case where a trailer was selected but it was the same as the current trailer
        //Since no safety check logic is required the user is moved directly onto the comment screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToClausedScreen_SkipClauseScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, false, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;
            mobileData.Order.Additional.Trailer.TrailerId = startUpService.CurrentTrailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no safety check logic was required, the comment view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        //Tests the case where a trailer was selected and since it differs from the current trailer
        //the user is directed to the safety check screen 
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToSafetyCheckScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);
            var navData = new NavData<MobileData>() { Data = mobileData };

            // different trailer
            navData.OtherData["UpdatedTrailer"] = _fixture.Create<Core.Models.Trailer>();

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            // Should go to safety check screen
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSafetyCheckViewModel), request.ViewModelType);
        }


        // Tests the case where a trailer was selected that is the same as the current railer but differs from the one currently on the order
        // Since no safety check is required the user is directed to the comments scren but the order is updated and the revised trailer data chunk sent 
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_Trailer_ToCommentScreen_UpdateOrder()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no safety check logic was required, the comment view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpService.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }


        //Tests the case where a trailer was selected but it was the same as the current trailer, comment bypass is enabled and
        //a signature is required
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();


            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        //Tests the case where a trailer was selected but it was the same as the current trailer (i.e no safety check logic)
        //and the bypass comment option is enabled
        public void NavigationService_Mappings_Instructions_Collection_TrailerToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, null);


            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }


        // Tests the case on the safety check screen where no safety check profile was detected for the updated trailer, so no signature is required so the comment screen can be navigated to
        // Since the selected trailer differs from the one on the order then the order is updated and the revised trailer chunk set
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_SafetyCheck_ToCommentScreen_UpdateOrder()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will be empty (so no safety profile can be retrieved for the trailer)
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>());

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no safety check logic was required, the comment view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpService.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));
        }


        // Tests the case on the safety check screen where a safety check has been completed so the user should be directed to the
        // safety check signature screen
        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_SafetyCheck_ToSafetyCheckSignatureScreen()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var startUpService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startUpService);

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = startUpService.CurrentTrailer;

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will return a safety profile for the trailer
            var safetyCheckProfile = _fixture.Create<SafetyProfile>();
            safetyCheckProfile.IntLink = startUpService.CurrentTrailer.SafetyCheckProfileIntLink;
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>() { safetyCheckProfile});

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that, since no safety check logic was required, the comment view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSafetyCheckSignatureViewModel), request.ViewModelType);

          
        }

        // Tests that that when the instruction safety check signature screen is completed the user is directed onto the comments scrteen
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_SafetyCheckSignature_ToCommentScreen()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckSignatureViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();

            startUpServiceMock.SetupProperty(x => x.CurrentTrailer);
            startUpServiceMock.SetupProperty(x => x.CurrentTrailerSafetyCheckData);

            startUpServiceMock.Object.CurrentTrailer = _fixture.Create<Core.Models.Trailer>();

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            var updatedSafetyCheckData = navData.OtherData["UpdatedTrailerSafetyCheckData"] = _fixture.Create<SafetyCheckData>();

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will be empty (so no safety profile can be retrieved for the trailer)
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>());

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the comment screen is navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));

            // should have set the current trailer
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer, trailer);

            // should have set the safety check data for the trailer
            Assert.Equal(updatedSafetyCheckData, startUpServiceMock.Object.CurrentTrailerSafetyCheckData);

            // check the safety check data was commited
            startUpServiceMock.Verify(ss => ss.CommitSafetyCheckData());
        }

        // Tests that that when the instruction safety check signature screen is completed and there are faults the user is directed 
        // back to the instruction screen
        [Fact]
        public void NavigationService_Mappings_Instructions_ChangeTrailer_SafetyCheckSignature_FailedChecks_ToInstructionScreen()
        {
            base.ClearAll();


            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));

            // presenter will report the current activity view model as MainView, current fragment model as a the instruction safety check 
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionSafetyCheckSignatureViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();

            startUpServiceMock.SetupProperty(x => x.CurrentTrailer);
            startUpServiceMock.SetupProperty(x => x.CurrentTrailerSafetyCheckData);

            startUpServiceMock.Object.CurrentTrailer = _fixture.Create<Core.Models.Trailer>();

            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;

            SafetyCheckData updatedSafetyCheckData;
            navData.OtherData["UpdatedTrailerSafetyCheckData"] = updatedSafetyCheckData = _fixture.Create<SafetyCheckData>();
            //add a failed check
            updatedSafetyCheckData.Faults.Add(new SafetyCheckFault() { Status = SafetyCheckStatus.Failed });

            var service = _fixture.Create<NavigationService>();

            //safety profile repository will be empty (so no safety profile can be retrieved for the trailer)
            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(new List<SafetyProfile>());

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the instruction screen is navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);

            // should have updated the trailer on the order
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer.Registration, mobileData.Order.Additional.Trailer.TrailerId);

            // datachunk service should have been hit to send the revised trailer data chunk
            _mockDataChunkService.Verify(mds => mds.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(), It.Is<MobileData>(md => md == mobileData), It.IsAny<Driver>(), It.IsAny<Vehicle>(), It.Is<bool>(b => !b), It.Is<bool>(b => b)));

            // should have set the current trailer
            Assert.Equal(startUpServiceMock.Object.CurrentTrailer, trailer);

            // should have set the safety check data for the trailer
            Assert.Equal(updatedSafetyCheckData, startUpServiceMock.Object.CurrentTrailerSafetyCheckData);

            // check the safety check data was commited
            startUpServiceMock.Verify(ss => ss.CommitSafetyCheckData());
        }

        #endregion

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_CommentToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionCommentViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_CommentToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionCommentViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_InstructionOnSiteSkipTrailerScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, true, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_InstructionOnSiteToInstructionClaused()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, false, false, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionClausedViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_InstructionOnSiteToCommentScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_InstructionOnSiteToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, true, true, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_InstructionOnSiteToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_ClausedToComment()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionClausedViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_ClausedToSignature()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionClausedViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, true, true, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_ClausedToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionClausedViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }
        
        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_CommentToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionCommentViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, true, true, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Delivery_CommentToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionCommentViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_OrderToReviseQuantity()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an order view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OrderViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, false, false, true, null);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the Revise Quantity screen was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ReviseQuantityViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_OrderToInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an order view view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OrderViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, false, false, true, MWF.Mobile.Core.Enums.InstructionProgress.Driving);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the Instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_OrderToInstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OrderViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, false, false, true, MWF.Mobile.Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_InstructionOnSiteToInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = null };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_InstructionToManifest()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();


            // Move to the next view model
            service.GoBack();

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_SkipCommentToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction trailer view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _mockUserInteraction.Setup(mui => mui.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync<IUserInteraction, bool>(false);

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, null);

            var trailer = _fixture.Create<MWF.Mobile.Core.Models.Trailer>();
            var startUpServiceMock = _fixture.InjectNewMock<IStartupService>();
            startUpServiceMock.Setup(ss => ss.CurrentTrailer).Returns(trailer);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };
            navData.OtherData["UpdatedTrailer"] = trailer;
            mobileData.Order.Additional.Trailer.TrailerId = trailer.Registration;

            var service = _fixture.Create<NavigationService>();

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Camera_NoInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as a camera view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = null };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Camera_Instruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Camera_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as a camera view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_Camera_NoInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileDataRepositoryMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepositoryMock.Setup(mdr => mdr.GetByID(It.IsAny<Guid>())).Returns((MobileData)null);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = null };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_Camera_Instruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_Camera_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<SidebarCameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_DisplaySafetyCheck_NoInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);



            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = null };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_DisplaySafetyCheck_Instruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = _mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_DisplaySafetyCheck_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_DisplaySafetyCheck_NoInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);




            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = null };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_DisplaySafetyCheck_Instruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.GoBack(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_DisplaySafetyCheck_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<DisplaySafetyCheckViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionOnSiteViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Manifest_TrunkTo()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.TrunkTo;

            var manifestViewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            // presenter will report the current activity view model as a MainViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == manifestViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavData<MobileData> parametersObjectIn = new NavData<MobileData> { Data = _mobileData };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrunkProceedViewModel), request.ViewModelType);


        }

        [Fact]
        public void NavigationService_Mappings_Manifest_ProceedFrom()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.ProceedFrom;

            var manifestViewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            // presenter will report the current activity view model as a MainViewModel,  current fragment model a passcode model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == manifestViewModel);
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavData<MobileData> parametersObjectIn = new NavData<MobileData> { Data = _mobileData };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrunkProceedViewModel), request.ViewModelType);


        }

        [Fact]
        public void NavigationService_Mappings_TrunkTo_TrailerSelect()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.TrunkTo;

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrunkProceedViewModel>());

            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Change Trailer?");

            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavData<MobileData> navData = new NavData<MobileData> { Data = _mobileData };
            _mobileData.Order.Additional.IsTrailerConfirmationEnabled = true;

            service.MoveToNext(navData);

            //Check that the trailer select screen was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrailerViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_TrunkTo_Manifest()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.TrunkTo;

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrunkProceedViewModel>());

            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavData<MobileData> navData = new NavData<MobileData> { Data = _mobileData };
            _mobileData.Order.Additional.IsTrailerConfirmationEnabled = false;

            service.MoveToNext(navData);

            //Check that the startup the manifest view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_Mappings_OnSite_BarcodeScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            //Skip trailer selection and go straight to barcode screen
            _mockUserInteraction.ConfirmAsyncReturnsFalseIfTitleStartsWith("Change Trailer?");

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, true, true, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navData = new NavData<MobileData>() { Data = mobileData };

            // Move to the next view model
            service.MoveToNext(navData);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(BarcodeScanningViewModel), request.ViewModelType);
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

            var safetyProfileRepositoryMock = _fixture.InjectNewMock<ISafetyProfileRepository>();
            safetyProfileRepositoryMock.Setup(s => s.GetAll()).Returns(new List<SafetyProfile>() {safetyProfile}) ;

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
            var safetyProfileRepositoryMock = _fixture.InjectNewMock<ISafetyProfileRepository>();
            safetyProfileRepositoryMock.Setup(s => s.GetAll()).Returns(new List<SafetyProfile>());

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
