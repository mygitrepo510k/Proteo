using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
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
        private Mock<IMobileDataRepository> _mobileDataRepo;
        private Mock<INavigationService> _navigationService;
        private IMainService _mainService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mainService = _fixture.Create<IMainService>();
            _fixture.Inject<IMainService>(_mainService);

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void InstructionCommmentVM_CommentAddToMobileApplicationDataChunkService()
        {
            base.ClearAll();

            var InstructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            InstructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            InstructionCommentVM.CommentText = "This is a test comment";

            InstructionCommentVM.AdvanceInstructionCommentCommand.Execute(null);

            Assert.Equal(InstructionCommentVM.CommentText, _mainService.CurrentDataChunkActivity.Comment);

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

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, false, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionCommentVM_NavButton_Deliver_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, true, false, null);

            var instructionCommentVM = _fixture.Create<InstructionCommentViewModel>();

            instructionCommentVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionCommentVM.InstructionCommentButtonLabel);
        }

        #endregion Tests
    }
}
