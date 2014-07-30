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
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Presentation;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class NavigationServiceTests
        : MvxIoCSupportingTest
    {


        private IFixture _fixture;
        private MockDispatcher _mockViewDispatcher;

        protected override void AdditionalSetup()
        {
            _mockViewDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(_mockViewDispatcher);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

        }

        [Fact]
        public void NavigationService_InsertNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to PasscodeViewModel
            service.InsertNavAction<StartupViewModel, CustomerCodeViewModel>(typeof(PasscodeViewModel));

            Assert.True(service.NavActionExists<StartupViewModel, CustomerCodeViewModel>());

        }

        [Fact]
        public void NavigationService_NavActionExists_NoNavActionDefined()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Assert.False(service.NavActionExists<StartupViewModel, CustomerCodeViewModel>());

        }

        [Fact]
        public void NavigationService_GetNavAction()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to PasscodeViewModel
            service.InsertNavAction<StartupViewModel, CustomerCodeViewModel>(typeof(PasscodeViewModel));

            Action navAction = service.GetNavAction<StartupViewModel, CustomerCodeViewModel>();

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(PasscodeViewModel), request.ViewModelType);

        }


        [Fact]
        public void NavigationService_GetNavAction_DynamicOverload()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to PasscodeViewModel
            service.InsertNavAction<StartupViewModel, CustomerCodeViewModel>(typeof(PasscodeViewModel));

            Action navAction = service.GetNavAction(typeof(StartupViewModel), typeof(CustomerCodeViewModel));

            Assert.NotNull(navAction);

            // run the nav action
            navAction.Invoke();

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(PasscodeViewModel), request.ViewModelType);

        }

        [Fact]
        public void NavigationService_GetNavAction_NoNavActionDefined()
        {
            base.ClearAll();

            var service = _fixture.Create<NavigationService>();

            Action navAction = service.GetNavAction<StartupViewModel, CustomerCodeViewModel>();

            Assert.Null(navAction);

        }


        [Fact]
        public void NavigationService_MoveToNext()
        {
            base.ClearAll();

            // presenter will report the current activity view model as the startup view model
            var mockCustomPresenter = Mock.Of<ICustomPresenter>(cp => cp.CurrentActivityViewModel == _fixture.Create<StartupViewModel>());
            _fixture.Inject<ICustomPresenter>(mockCustomPresenter);

            var customerCodeModel = _fixture.Create<CustomerCodeViewModel>();

            var service = _fixture.Create<NavigationService>();

            // Specify that from StartUp/CustomerCode view model we should navigate to PasscodeViewModel
            service.InsertNavAction<StartupViewModel, CustomerCodeViewModel>(typeof(PasscodeViewModel));

            // Move to the next view model
            service.MoveToNextFrom(customerCodeModel);

            //Check that the passcode view model was navigated to
            Assert.Equal(1, _mockViewDispatcher.Requests.Count);
            var request = _mockViewDispatcher.Requests.First();
            Assert.Equal(typeof(PasscodeViewModel), request.ViewModelType);

        }
       

    }

}
