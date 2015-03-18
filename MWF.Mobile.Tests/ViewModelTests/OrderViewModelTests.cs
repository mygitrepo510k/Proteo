using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
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
        private Mock<IMainService> _mockMainService;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockMainService = _fixture.InjectNewMock<IMainService>();

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());
            Ioc.RegisterSingleton<INavigationService>(_navigationService.Object);

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void OrderVM_OrderID()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().ItemIdFormatted, orderVM.OrderID);

        }

        [Fact]
        public void OrderVM_Title()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Title, orderVM.OrderLoadNo);

        }

        [Fact]
        public void OrderVM_DeliveryOrderNumber()
        {
            base.ClearAll();
            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().DeliveryOrderNumber, orderVM.OrderDeliveryNo);

        }

        [Fact]
        public void OrderVM_Quantity()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();
            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Quantity, orderVM.OrderQuantity);

        }

        [Fact]
        public void OrderVM_Weight()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Weight, orderVM.OrderWeight);

        }

        [Fact]
        public void OrderVM_BusinessType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().BusinessType, orderVM.OrderBusinessType);

        }

        [Fact]
        public void OrderVM_GoodsType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().GoodsType, orderVM.OrderGoodsType);

        }

        [Fact]
        public void OrderVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);
            orderVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void OrderVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var orderVM = _fixture.Create<OrderViewModel>();

            var navData = new NavData<Item>() { Data = _mobileData.Order.Items.FirstOrDefault() };
            navData.OtherData["MobileData"] = _mobileData;

            orderVM.Init(navData);

            orderVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

        }

        #endregion Tests
    }
}
