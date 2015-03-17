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

            _mockNavigationService.Verify(mns => mns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Once);
        }

        [Fact]
        public void InstructionTrunkProceedVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            InstructionTrunkProceedVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockNavigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void InstructionTrunkProceedVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var InstructionTrunkProceedVM = _fixture.Create<InstructionTrunkProceedViewModel>();

            InstructionTrunkProceedVM.Init(new NavData<MobileData>() { Data = _mobileData });

            _mobileData.GroupTitle = "UpdateTitle";

            InstructionTrunkProceedVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

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
