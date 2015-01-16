using Cirrious.MvvmCross.Community.Plugins.Sqlite;
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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionTrunkToViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;
        private Mock<IMainService> _mockMainService;



        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";
            _mobileData.Order.Type = InstructionType.TrunkTo;

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());

            _mockMainService = _fixture.InjectNewMock<IMainService>();
            _mockMainService.Setup(m => m.CurrentMobileData).Returns(_mobileData);

        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionTrunkToVM_FragmentTitle()
        {
            base.ClearAll();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Trunk To", InstructionTrunkToVM.FragmentTitle);

        }

        [Fact]
        public void InstructionTrunkToVM_RunID()
        {
            base.ClearAll();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.RouteTitle, InstructionTrunkToVM.RunID);

        }

        [Fact]
        public void InstructionTrunkToVM_ArriveDateTime()
        {
            base.ClearAll();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Arrive.ToString(), InstructionTrunkToVM.ArriveDateTime);

        }

        [Fact]
        public void InstructionTrunkToVM_ArriveDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Arrive = new DateTime();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(string.Empty, InstructionTrunkToVM.ArriveDateTime);

        }

        [Fact]
        public void InstructionTrunkToVM_DepartDateTime()
        {
            base.ClearAll();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Depart.ToString(), InstructionTrunkToVM.DepartDateTime);

        }

        [Fact]
        public void InstructionTrunkToVM_DepartDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Depart = new DateTime();

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(string.Empty, InstructionTrunkToVM.DepartDateTime);

        }

        [Fact]
        public void InstructionTrunkToVM_Address()
        {
            base.ClearAll();

            _mobileData.Order.Addresses[0].Lines = "Testline1|TestLine2|TestLine3";

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode, InstructionTrunkToVM.Address);

        }

        [Fact]
        public void InstructionTrunkToVM_CompleteButton()
        {
            base.ClearAll();

            var instructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            instructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionTrunkToVM.CompleteInstructionCommand.Execute(null);

            _mockNavigationService.Verify(mns => mns.MoveToNext(It.IsAny<NavItem<MobileData>>()), Times.Once);
        }

        [Fact]
        public void InstructionTrunkToVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            InstructionTrunkToVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockNavigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void InstructionTrunkToVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var InstructionTrunkToVM = _fixture.Create<InstructionTrunkToViewModel>();

            InstructionTrunkToVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            _mobileData.GroupTitle = "UpdateTitle";

            InstructionTrunkToVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(2));

        }
        #endregion Test

    }
}
