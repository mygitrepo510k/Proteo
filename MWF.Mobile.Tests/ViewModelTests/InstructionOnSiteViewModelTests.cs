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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionOnSiteViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<INavigationService> _navigationService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";

            _navigationService = _fixture.InjectNewMock<INavigationService>();

        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionOnSiteVM_FragmentTitle_Collect()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>());

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Collect On Site", instructionOnSiteVM.FragmentTitle);
        }

        [Fact]
        public void InstructionOnSiteVM_FragmentTitle_Deliver()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>());

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Deliver On Site", instructionOnSiteVM.FragmentTitle);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Collect_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, true, false, false, false);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Complete", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Deliver_Complete()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, true, false, false, false);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Complete", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Collect_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, false, true, false);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        [Fact]
        public void InstructionOnSiteVM_NavButton_Deliver_Continue()
        {
            base.ClearAll();

            _fixture.SetUpInstruction(Core.Enums.InstructionType.Deliver, false, false, true, false);

            var instructionOnSiteVM = _fixture.Create<InstructionOnSiteViewModel>();

            instructionOnSiteVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Continue", instructionOnSiteVM.InstructionCommentButtonLabel);
        }

        #endregion Test

    }
}
