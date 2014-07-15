﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;


namespace MWF.Mobile.Tests.ViewModelTests
{
    public class CustomerCodeViewModelTests : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Mock<IUserInteraction> _mockUserInteraction;
        private Mock<IDataService> _dataService;

        protected override void AdditionalSetup()
        {
            _mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(_mockUserInteraction.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _dataService = new Mock<IDataService>();
            _dataService.Setup(ds => ds.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a.Invoke());
            _fixture.Register<IDataService>(() => _dataService.Object);
        }

        [Fact]
        public void CustCodeVM_NoInternetDoesNotShowSpinner()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == false));

            var ccvm =_fixture.Create<CustomerCodeViewModel>();
            ccvm.CustomerCode = "123";
            ccvm.IsBusy = false;

            ccvm.EnterCodeCommand.Execute(null);

            Assert.Equal(false, ccvm.IsBusy);
        }

        [Fact]
        //Tests that when the customer code is invalid, an alert with an error message is shown
        public void CustCodeVM_InvalidCustomerCode()
        {
            base.ClearAll();

            // Get device returns null (i.e. customer code is invalid)
            _fixture.Register<IGatewayService>(() => Mock.Of<IGatewayService>(gs => gs.GetDevice(It.IsAny<string>()) == Task.FromResult<Device>((Device) null)  ));

            var ccvm = _fixture.Create<CustomerCodeViewModel>();

            // Enter the code
            ccvm.EnterCodeCommand.Execute(null);

            // check error message has returned
            _mockUserInteraction.Verify(ui => ui.AlertAsync(It.Is<string>(s => s == "Invalid customer code."), It.IsAny<string>(), It.IsAny<string>()), Times.Once());

        }

        [Fact]
        //Tests that when an exception is thrown by the gateway service (or sql service) an error message is displayed
        public void CustCodeVM_SetUpException()
        {
            base.ClearAll();

            // Get device throws an exception
            var gatewayService = new Mock<IGatewayService>();
            gatewayService.Setup(gs => gs.GetDevice(It.IsAny<string>())).Callback(() => { throw new Exception(); });
            _fixture.Register<IGatewayService>(() => gatewayService.Object);

            var ccvm = _fixture.Create<CustomerCodeViewModel>();

            // Enter the code
            ccvm.EnterCodeCommand.Execute(null);

            // check error message has returned
            _mockUserInteraction.Verify(ui => ui.AlertAsync(It.Is<string>(s => s == "Unable to sync customer settings to device."), It.IsAny<string>(), It.IsAny<string>()), Times.Once());

        }

        [Fact]
        // Tests that what when a valid customer code is entered
        // data is pulled from the web services, customer code is persisted in the db
        public void CustCodeVM_ValidCustomerCode()
        {
            base.ClearAll();

            Device dev = new Device();

            var gateWayService = Mock.Of<IGatewayService>(gs =>
                gs.GetDevice("123") == Task.FromResult<Device>(dev));

            _fixture.Register<IGatewayService>(() => gateWayService);

            var customerRepository = new Mock<ICustomerRepository>();
            _fixture.Inject<ICustomerRepository>(customerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var ccvm = _fixture.Create<CustomerCodeViewModel>();
            ccvm.CustomerCode = "123";


            // Enter the code
            ccvm.EnterCodeCommand.Execute(null);

            //check that the customer repository was written to
            customerRepository.Verify(cr => cr.Insert(It.IsAny<Customer>()), Times.Once);


        }



        
    }
}
