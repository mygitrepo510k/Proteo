using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Tests.Helpers;
using Xunit;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Portable;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionOnSiteViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<INavigationService> _navigationService; 
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<IMainService> _mockMainService;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockMainService = _fixture.InjectNewMock<IMainService>();
            _mockMainService.Setup(m => m.CurrentMobileData).Returns(_mobileData);

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());
            Ioc.RegisterSingleton<INavigationService>(_navigationService.Object);

        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionOnSiteVM_FragmentTitle_Collect()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Collect On Site", instructionOnSiteVM.FragmentTitle);
        }

        [Fact]
        public void InstructionOnSiteVM_FragmentTitle_Deliver()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = mobileData });

            Assert.Equal("Deliver On Site", instructionOnSiteVM.FragmentTitle);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Collect_Complete()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = mobileData });

            Assert.Equal("Complete", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Deliver_Complete()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = mobileData });

            Assert.Equal("Complete", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Collect_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, false, null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Continue", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Deliver_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, true, false, null);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Continue", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionOnSiteVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void InstructionOnSiteVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionOnSiteVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

        }

        #endregion Test

    }
}
