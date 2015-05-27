using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
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
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<IMainService> _mockMainService;
        private Mock<IDataChunkService> _mockDataChunkService;
        private Mock<IMvxMessenger> _mockMessenger;

        #endregion Private Members

        #region Setup

        protected override void AdditionalSetup()
        {

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();
            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Complete Instruction");

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockMainService = _fixture.InjectNewMock<IMainService>();

            _mockDataChunkService = _fixture.InjectNewMock<IDataChunkService>();

            _mockDataChunkService.Setup(m => m.SendDataChunk(It.IsAny<MobileApplicationDataChunkContentActivity>(),  It.IsAny<MobileData>(), It.IsAny<Driver>(), It.IsAny<Vehicle>(),false, false));


            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            Ioc.RegisterSingleton<INavigationService>(_navigationService.Object);
        }

        #endregion Setup

        #region Test

        [Fact]
        public void InstructionSignatureVM_Complete_Signature()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            var navData = new NavData<MobileData>() { Data = _mobileData };
            var dataChunk = _fixture.Create<MobileApplicationDataChunkContentActivity>();
            navData.OtherData["DataChunk"] = dataChunk;

            instructionSignatureVM.Init(navData);


            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny <NavData<MobileData>>()), Times.Once);

            Assert.Same(instructionSignatureVM.CustomerSignatureEncodedImage, dataChunk.Signature.EncodedImage);
            Assert.Same(instructionSignatureVM.CustomerName, dataChunk.Signature.Title);

        }

        [Fact]
        public void InstructionSignatureVM_NullSignatureCheck_Collect()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = true;

            instructionSignatureVM.CustomerSignatureEncodedImage = "";

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullSignatureCheck_Deliver()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery = true;

            instructionSignatureVM.CustomerSignatureEncodedImage = "";

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullCustomerNameCheck_Collect()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.CustomerNameRequiredForCollection = true;

            instructionSignatureVM.CustomerName = "";

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM_NullCustomerNameCheck_Deliver()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;
            _mobileData.Order.Additional.CustomerNameRequiredForDelivery = true;

            instructionSignatureVM.CustomerName = "";

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.InstructionDoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Never);
        }

        [Fact]
        public void InstructionSignatureVM__Collect_FragmentTitle()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Sign for Collection", instructionSignatureVM.FragmentTitle);

        }

        [Fact]
        public void InstructionSignatureVM_Deliver_FragmentTitle()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Sign for Delivery", instructionSignatureVM.FragmentTitle);

        }

        [Fact]
        public void InstructionSignatureVM_SignatureUnavailableToggleButtonText()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.IsSignaturePadEnabled = false;

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Signature available", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_SignatureAvailableToggleButtonText()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.IsSignaturePadEnabled = true;

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Signature unavailable", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_Collect_SignatureToggleButtonDisabled()
        {
            base.ClearAll();

            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = true;
            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });
         
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

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

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

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });


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

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal(false, instructionSignatureVM.IsSignaturePadEnabled);
            Assert.Equal(true, instructionSignatureVM.IsSignatureToggleButtonEnabled);

            Assert.Equal("Signature available", instructionSignatureVM.SignatureToggleButtonLabel);

        }

        [Fact]
        public void InstructionSignatureVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void InstructionSignatureVM_CheckInstructionNotification_Update_Confirm()
        {
            base.ClearAll();

            var instructionSignatureVM = _fixture.Create<InstructionSignatureViewModel>();

            instructionSignatureVM.Init(new NavData<MobileData>() { Data = _mobileData });

            instructionSignatureVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == _mobileData.ID.ToString())), Times.Exactly(1));

        }


        #endregion Test
    }
}
