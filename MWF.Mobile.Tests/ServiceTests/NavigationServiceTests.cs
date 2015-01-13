﻿using System;
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

           

        protected override void AdditionalSetup()
        {

            _mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();

            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Complete Instruction");

            _mockUserInteraction.Setup(mui => mui.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync<IUserInteraction,bool>(true);
          
            Ioc.RegisterSingleton<IMvxStringToTypeParser>(new MvxStringToTypeParser());

            _mockViewDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(_mockViewDispatcher);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mockMessenger = new Mock<IMvxMessenger>();
            Ioc.RegisterSingleton<IMvxMessenger>(_mockMessenger.Object);

            _mobileData = _fixture.Create<MobileData>();

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

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

            bool testBool = false;

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertCustomNavAction<ActivityViewModel, FragmentViewModel1>( (a) => testBool = (bool) a );

            Assert.True(service.NavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action<Object> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

            navAction.Invoke(true);

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
            service.InsertCustomBackNavAction<ActivityViewModel, FragmentViewModel1>((a) => testBool = (bool) a);

            Assert.True(service.BackNavActionExists<ActivityViewModel, FragmentViewModel1>());

            Action<Object> navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

            navAction.Invoke(true);

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

            Action<Object> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

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

            Action<Object> navAction = service.GetBackNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel2));

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

            Action<Object> navAction = service.GetNavAction(typeof(ActivityViewModel), typeof(FragmentViewModel1));

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

            Action<Object> navAction = service.GetNavAction<ActivityViewModel, FragmentViewModel1>();

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
        public void NavigationService_MoveToNext_WithParameters()
        {
            base.ClearAll();

            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<ActivityViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<FragmentViewModel1>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to FragmentViewModel2
            service.InsertNavAction<ActivityViewModel, FragmentViewModel1>(typeof(FragmentViewModel2));

            //Create an object to pass through as a parameter 
            NavItem<Driver> parametersObjectIn = new NavItem<Driver> { ID = Guid.NewGuid() };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(FragmentViewModel2), request.ViewModelType);

            // check that the supplied parameters were passed through correctly
            var parametersObjectOut = request.ParameterValues.First();
            Assert.Equal("ID", parametersObjectOut.Key);
            Assert.Equal(parametersObjectIn.ID.ToString(), parametersObjectOut.Value);

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
            Assert.Equal(1, _mockViewDispatcher.Hints.Count);
            var hint = _mockViewDispatcher.Hints.First();
            Assert.Equal(typeof(PasscodeViewModel), (hint as CloseUpToViewPresentationHint).ViewModelType);

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
            NavItem<MobileData> parametersObjectIn = new NavItem<MobileData> { ID = _mobileData.ID };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);

            // chck that the supplied parameters were passed through correctly
            var parametersObjectOut = request.ParameterValues.First();
            Assert.Equal("ID", parametersObjectOut.Key);
            Assert.Equal(parametersObjectIn.ID.ToString(), parametersObjectOut.Value);

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
            var navItemMock = Mock.Of<NavItem<Item>>();

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

            var navItemMock = Mock.Of<NavItem<MWF.Mobile.Core.Models.Instruction.Trailer>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            var navItemMock = Mock.Of<NavItem<MobileData>>();

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
            var navItemMock = Mock.Of<NavItem<Item>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the order view  model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(OrderViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_InstructionOnSiteToTrailerScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionOnSiteViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrailerViewModel), request.ViewModelType);
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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MWF.Mobile.Core.Models.Instruction.Trailer>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToCommentScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_TrailerToComplete()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Instructions_Collection_CommentToSignatureScreen()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionCommentViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, true, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionCommentViewModel), request.ViewModelType);
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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_OrderToReviseQuantity()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OrderViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ReviseQuantityViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_BackAction_OrderToInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<OrderViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, MWF.Mobile.Core.Enums.InstructionProgress.Driving);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

            //Check that the trailer list view model was navigated to
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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, true, false, false, MWF.Mobile.Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrailerViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            _mockUserInteraction.Setup(mui => mui.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync<IUserInteraction, bool>(false);

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, null);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionSignatureViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Camera_NoInstruction()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileDataRepositoryMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepositoryMock.Setup(mdr => mdr.GetByID(It.IsAny<Guid>())).Returns((MobileData)null);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

            //Check that the trailer list view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionViewModel), request.ViewModelType);
        }

        [Fact]
        public void NavigationService_Mappings_Camera_InstructionOnSite()
        {
            base.ClearAll();

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var mobileDataRepositoryMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepositoryMock.Setup(mdr => mdr.GetByID(It.IsAny<Guid>())).Returns((MobileData)null);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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
                                                                cp.CurrentFragmentViewModel == _fixture.Create<CameraViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

            //Check that the trailer list view model was navigated to
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


            var mobileDataRepositoryMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepositoryMock.Setup(mdr => mdr.GetByID(It.IsAny<Guid>())).Returns((MobileData)null);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.MoveToNext(navItemMock);

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


            var mobileDataRepositoryMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepositoryMock.Setup(mdr => mdr.GetByID(It.IsAny<Guid>())).Returns((MobileData)null);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.NotStarted);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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


            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, Core.Enums.InstructionProgress.OnSite);

            var service = _fixture.Create<NavigationService>();

            var navItemMock = Mock.Of<NavItem<MobileData>>();

            // Move to the next view model
            service.GoBack(navItemMock);

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
            NavItem<MobileData> parametersObjectIn = new NavItem<MobileData> { ID = _mobileData.ID };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(InstructionTrunkToViewModel), request.ViewModelType);

            // chck that the supplied parameters were passed through correctly
            var parametersObjectOut = request.ParameterValues.First();
            Assert.Equal("ID", parametersObjectOut.Key);
            Assert.Equal(parametersObjectIn.ID.ToString(), parametersObjectOut.Value);

        }

        [Fact]
        public void NavigationService_Mappings_TrunkTo_Manifest()
        {
            base.ClearAll();

            _mobileData.Order.Type = InstructionType.TrunkTo;

            // presenter will report the current activity view model as MainView, current fragment model as an instruction view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp =>
                                                                cp.CurrentActivityViewModel == _fixture.Create<MainViewModel>() &&
                                                                cp.CurrentFragmentViewModel == _fixture.Create<InstructionTrunkToViewModel>());

            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);


            var service = _fixture.Create<NavigationService>();

            //Create a nav item for a mobile data model
            NavItem<MobileData> parametersObjectIn = new NavItem<MobileData> { ID = _mobileData.ID };
            service.MoveToNext(parametersObjectIn);

            //Check that the startup the instruction view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(ManifestViewModel), request.ViewModelType);

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
