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
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IMvxMessenger> _mockMessenger;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            Ioc.RegisterSingleton<INavigationService>(_navigationService.Object);

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void InstructionCommmentVM_CommentAddToMobileApplicationDataChunkService()
        {
            base.ClearAll();

            var InstructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            var navData = new NavData<MobileData>() { Data = _mobileData };
            var dataChunk = _fixture.Create<MobileApplicationDataChunkContentActivity>();
            navData.OtherData["DataChunk"] = dataChunk;

            InstructionCommentVM.Init(navData);

            var inputText = "This is a test comment";

            InstructionCommentVM.CommentText = inputText;

            var mockDataChunkActivity = _fixture.Create<MobileApplicationDataChunkContentActivity>();
            mockDataChunkActivity.Comment = inputText;


            InstructionCommentVM.AdvanceInstructionCommentCommand.Execute(null);

            Assert.Equal(dataChunk.Comment, InstructionCommentVM.CommentText);   
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Collect_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false, false, false,true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Complete", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Deliver_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false, false, false, true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Complete", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Collect_Continue()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, true, false, false, true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavData<MobileData>() { Data = mobileData });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Deliver_Continue()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, true, true, false, false, true, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavData<MobileData>() { Data = mobileData });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_CheckInstructionNotification_Delete()
        {
            base.ClearAll();

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();
            instructionCommentVM.IsVisible = true;

            instructionCommentVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionCommentVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }

        [Fact]
        public void InstructionCommentVM_CheckInstructionNotification_Update_Confirm()
        {
            base.ClearAll();

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();
            instructionCommentVM.IsVisible = true;

            instructionCommentVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionCommentVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

        }


        #endregion Tests
    }
}
