﻿using SQLite.Net.Attributes;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
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
using MWF.Mobile.Core.Repositories.Interfaces;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Extensions;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionTrunkProceedViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IInfoService> _mockInfoService;
        private Mock<IMvxMessenger> _mockMessenger;


        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";
            _mobileData.Order.Type = InstructionType.TrunkTo;

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByIDAsync(It.Is<Guid>(i => i == _mobileData.ID))).ReturnsAsync(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());


            _mockInfoService = _fixture.InjectNewMock<IInfoService>();

            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);

        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionTrunkProceedVM_FragmentTitle()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Trunk To", InstructionTrunkProceedVM.FragmentTitle);

        }

        [Fact]
        public void InstructionTrunkProceedVM_RunID()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(_mobileData.Order.RouteTitle, InstructionTrunkProceedVM.RunID);

        }

        [Fact]
        public void InstructionTrunkProceedVM_ProceedFromDepartDateTime()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            _mobileData.Order.Type = InstructionType.ProceedFrom;

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(_mobileData.Order.Arrive.ToStringIgnoreDefaultDate(), InstructionTrunkProceedVM.ArriveDepartDateTime);
            Assert.Equal("Depart", InstructionTrunkProceedVM.ArriveDepartLabelText);

        }

        [Fact]
        public void InstructionTrunkProceedVM_ProceedFromDepartDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Arrive = new DateTime();
            _mobileData.Order.Type = InstructionType.ProceedFrom;

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(string.Empty, InstructionTrunkProceedVM.ArriveDepartDateTime);

        }

        [Fact]
        public void InstructionTrunkProceedVM_TrunkArriveDateTime()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            _mobileData.Order.Type = InstructionType.TrunkTo;

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(_mobileData.Order.Arrive.ToStringIgnoreDefaultDate(), InstructionTrunkProceedVM.ArriveDepartDateTime);
            Assert.Equal("Arrive", InstructionTrunkProceedVM.ArriveDepartLabelText);

        }

        [Fact]
        public void InstructionTrunkProceedVM_TrunkArriveDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Arrive = new DateTime();
            _mobileData.Order.Type = InstructionType.TrunkTo;

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(string.Empty, InstructionTrunkProceedVM.ArriveDepartDateTime);

        }

        [Fact]
        public void InstructionTrunkProceedVM_Address()
        {
            base.ClearAll();

            _mobileData.Order.Addresses[0].Lines = "Testline1|TestLine2|TestLine3";

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(_mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode, InstructionTrunkProceedVM.Address);

        }

        [Fact]
        public void InstructionTrunkProceedVM_CompleteButton()
        {
            base.ClearAll();

            var instructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            instructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionTrunkProceedVM.CompleteInstructionCommand.Execute(null);

            _mockNavigationService.Verify(mns => mns.MoveToNextAsync(It.IsAny<NavData<MobileData>>()), Times.Once);
        }

        [Fact]
        public void InstructionTrunkProceedVM_CheckInstructionNotification_Delete()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            InstructionTrunkProceedVM.IsVisible = true;

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            InstructionTrunkProceedVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockNavigationService.Verify(ns => ns.GoToManifestAsync(), Times.Once);

        }


        [Fact]
        public void InstructionTrunkProceedVM_CheckInstructionNotification_Update_Confirm()
        {
            base.ClearAll();

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            InstructionTrunkProceedVM.IsVisible = true;

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            _mobileData.GroupTitle = "UpdateTitle";

            InstructionTrunkProceedVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByIDAsync(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

        }

        [Fact]
        public void InstructionTrunkProceedVM_FragmentTitle_Proceed()
        {
            base.ClearAll();

            var instructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            _mobileData.Order.Type = InstructionType.ProceedFrom;

            instructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Proceed From", instructionTrunkProceedVM.FragmentTitle);

        }

        [Fact]
        public void InstructionTrunkProceedVM_FragmentTitle_Trunk()
        {
            base.ClearAll();

            var instructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();
            _mobileData.Order.Type = InstructionType.TrunkTo;

            instructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Trunk To", instructionTrunkProceedVM.FragmentTitle);

        }


        #endregion Test

    }
}
