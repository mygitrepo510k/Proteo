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
    public class InstructionCommentViewModelTests
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

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void InstructionCommmentVM_CommentAddToMobileApplicationDataChunkService()
        {
            base.ClearAll();

            var InstructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            InstructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            var inputText = "This is a test comment";

            InstructionCommentVM.CommentText = inputText;

            var mockDataChunkActivity = _fixture.Create<MobileApplicationDataChunkContentActivity>();
            mockDataChunkActivity.Comment = inputText;

            _mockMainService.Setup(ms => ms.CurrentDataChunkActivity).Returns(mockDataChunkActivity);

            InstructionCommentVM.AdvanceInstructionCommentCommand.Execute(null);

            Assert.Equal(_mockMainService.Object.CurrentDataChunkActivity.Comment, InstructionCommentVM.CommentText);   
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Collect_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Complete", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Deliver_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Complete", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Collect_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Deliver_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, true, true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionCommentVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void InstructionCommentVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionCommentVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(2));

        }

        #endregion Tests
    }
}
