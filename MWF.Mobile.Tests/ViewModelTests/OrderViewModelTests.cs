﻿using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
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
    public class OrderViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<INavigationService> _navigationService;
        private Mock<IInfoService> _mockInfoService;
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IMvxMessenger> _mockMessenger;
        private Mock<IConfigRepository> _mockConfigRepo;


        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByIDAsync(It.Is<Guid>(i => i == _mobileData.ID))).ReturnsAsync(_mobileData);

            _mockConfigRepo = _fixture.InjectNewMock<IConfigRepository>();

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockInfoService = _fixture.InjectNewMock<IInfoService>();

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            Ioc.RegisterSingleton<INavigationService>(_navigationService.Object);

        }

        #endregion Setup

        #region Tests

        [Fact]
        public async Task OrderVM_OrderID()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().ItemIdFormatted, orderVM.OrderID);

        }

        [Fact]
        public async Task OrderVM_Title()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Title, orderVM.OrderLoadNo);

        }

        [Fact]
        public async Task OrderVM_DeliveryOrderNumber()
        {
            base.ClearAll();
            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().DeliveryOrderNumber, orderVM.OrderDeliveryNo);

        }

        [Fact]
        public async Task OrderVM_Quantity()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Quantity, orderVM.OrderQuantity);

        }

        [Fact]
        public async Task OrderVM_Weight()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Weight, orderVM.OrderWeight);

        }

        [Fact]
        public async Task OrderVM_BusinessType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().BusinessType, orderVM.OrderBusinessType);

        }

        [Fact]
        public async Task OrderVM_GoodsType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().GoodsType, orderVM.OrderGoodsType);

        }

        [Fact]
        public async Task OrderVM_Collection_QuantityEditable()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var config = _fixture.Create<MWFMobileConfig>();
            config.QuantityIsEditable = true;

            _mockConfigRepo.Setup(mcr => mcr.GetByIDAsync(It.IsAny<Guid>())).ReturnsAsync(config);

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(true, orderVM.ChangeOrderQuantity);
        }

        [Fact]
        public async Task OrderVM_Collection_QuantityNotEditable()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var config = _fixture.Create<MWFMobileConfig>();
            config.QuantityIsEditable = false;

            _mockConfigRepo.Setup(mcr => mcr.GetByIDAsync(It.IsAny<Guid>())).ReturnsAsync(config);

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(false, orderVM.ChangeOrderQuantity);
        }


        [Fact]
        public async Task OrderVM_Delivery_QuantityNotEditable()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var config = _fixture.Create<MWFMobileConfig>();

            _mockConfigRepo.Setup(mcr => mcr.GetByIDAsync(It.IsAny<Guid>())).ReturnsAsync(config);

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            Assert.Equal(false, orderVM.ChangeOrderQuantity);
        }

        [Fact]
        public async Task OrderVM_CheckInstructionNotification_Delete()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            orderVM.IsVisible = true;

            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);
            await orderVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifestAsync(), Times.Once);

        }


        [Fact]
        public async Task OrderVM_CheckInstructionNotification_Update_Confirm()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            orderVM.IsVisible = true;

            var navData = new NavData<MobileData>() { Data = _mobileData };
            navData.OtherData["Order"] = _mobileData.Order.Items.FirstOrDefault();

            await orderVM.Init(navData);

            await orderVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByIDAsync(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));
        }

        #endregion Tests
    }
}
