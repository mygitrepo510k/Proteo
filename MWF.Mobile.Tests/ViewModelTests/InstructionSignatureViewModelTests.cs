using Chance.MvvmCross.Plugins.UserInteraction;
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
    public class InstructionSignatureViewModelTests
        : MvxIoCSupportingTest
    {

        #region Private Members

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<INavigationService> _navigationService;
        private Mock<IUserInteraction> _mockUserInteraction;
        private Mock<IMobileDataRepository> _mobileDataRepo;
        private Mock<IMainService> _mainService;

        #endregion Private Members

        #region Setup

        protected override void AdditionalSetup()
        {

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _mockUserInteraction = Ioc.RegisterNewMock<IUserInteraction>();
            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Complete Instruction");

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            
            _mainService = _fixture.InjectNewMock<IMainService>();
            _mainService.Setup(m => m.CurrentDataChunkActivity).Returns(new MobileApplicationDataChunkContentActivity());
            _mainService.Setup(m => m.SendDataChunk(false));

            _navigationService = _fixture.InjectNewMock<INavigationService>();

        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionSignatureVM_Complete_Signature()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny <NavItem<MobileData>>()), Times.Once);

            Assert.Same(instructionSignatureVM.CustomerSignatureEncodedImage, _mainService.Object.CurrentDataChunkActivity.Signature.EncodedImage);
            Assert.Same(instructionSignatureVM.CustomerName, _mainService.Object.CurrentDataChunkActivity.Signature.Title);

        }

        [Fact]
        public void InstructionSignatureVM_NullSignatureCheck_Collect()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = true;

            instructionSignatureVM.CustomerSignatureEncodedImage = "";

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavItem<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullSignatureCheck_Deliver()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery = true;

            instructionSignatureVM.CustomerSignatureEncodedImage = "";

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavItem<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullCustomerNameCheck_Collect()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.CustomerNameRequiredForCollection = true;

            instructionSignatureVM.CustomerName = "";

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavItem<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullCustomerNameCheck_Deliver()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            _mobileData.Order.Additional.CustomerNameRequiredForDelivery = true;

            instructionSignatureVM.CustomerName = "";

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavItem<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM__Collect_FragmentTitle()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Sign for Collection", instructionSignatureVM.FragmentTitle);

        }

        [Fact]
        public void InstructionSignatureVM_Deliver_FragmentTitle()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Sign for Delivery", instructionSignatureVM.FragmentTitle);

        }

        [Fact]
        public void InstructionSignatureVM_SignatureUnavailableToggleButtonText()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.IsSignaturePadEnabled = false;

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Signature available", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_SignatureAvailableToggleButtonText()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.IsSignaturePadEnabled = true;

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Signature unavailable", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_Collect_SignatureToggleButtonDisabled()
        {
            base.ClearAll();

            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = true;
            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });
         
            Assert.Equal(false, instructionSignatureVM.IsSignatureToggleButtonEnabled);
            Assert.Equal(true, instructionSignatureVM.IsSignaturePadEnabled);

            Assert.Equal("Signature unavailable", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_Delivery_SignatureToggleButtonDisabled()
        {
            base.ClearAll();

            _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery = true;
            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(false, instructionSignatureVM.IsSignatureToggleButtonEnabled);
            Assert.Equal(true, instructionSignatureVM.IsSignaturePadEnabled);

            Assert.Equal("Signature unavailable", instructionSignatureVM.SignatureToggleButtonLabel);
            

        }

        [Fact]
        public void InstructionSignatureVM_Collect_SignatureToggleButtonEnabled()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = false;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });


            Assert.Equal(true, instructionSignatureVM.IsSignatureToggleButtonEnabled);
            Assert.Equal(false, instructionSignatureVM.IsSignaturePadEnabled);

            Assert.Equal("Signature available", instructionSignatureVM.SignatureToggleButtonLabel);
        }

        [Fact]
        public void InstructionSignatureVM_Delivery_SignatureToggleButtonEnabled()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery = false;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });


            
            Assert.Equal(false, instructionSignatureVM.IsSignaturePadEnabled);
            Assert.Equal(true, instructionSignatureVM.IsSignatureToggleButtonEnabled);

            Assert.Equal("Signature available", instructionSignatureVM.SignatureToggleButtonLabel);

        }


        #endregion Test
    }
}
