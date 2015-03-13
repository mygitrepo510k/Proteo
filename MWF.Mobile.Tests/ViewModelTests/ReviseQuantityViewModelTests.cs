﻿using Cirrious.MvvmCross.Plugins.Messenger;
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
    public class ReviseQuantityViewModelTests
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
            _mockMainService.Setup(m => m.CurrentMobileData).Returns(_mobileData);
            _mockMainService.Setup(m => m.CurrentDataChunkActivity).Returns(_fixture.Create<MobileApplicationDataChunkContentActivity>());

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void ReviseQuantityVM_SuccessfulUpdate()
        {
            base.ClearAll();

            var reviseQuantityVM = _fixture.Create<ReviseQuantityViewModel>();

            int newQuantity = 123;
            reviseQuantityVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            reviseQuantityVM.OrderQuantity = newQuantity.ToString();

            reviseQuantityVM.ReviseQuantityCommand.Execute(null);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Quantity, reviseQuantityVM.OrderQuantity);

        }

        [Fact]
        public void ReviseQuantityVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var reviseQuantityVM = _fixture.Create<ReviseQuantityViewModel>();

            reviseQuantityVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            reviseQuantityVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void ReviseQuantityVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var reviseQuantityVM = _fixture.Create<ReviseQuantityViewModel>();

            reviseQuantityVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            reviseQuantityVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(2));

        }

        #endregion Tests
    }
}