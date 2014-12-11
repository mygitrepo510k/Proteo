using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Moq;
using MWF.Mobile.Core.Models;
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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class DisplaySafetyCheckViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private LatestSafetyCheck _latestSafetyCheck;
        private MobileData _mobileData;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IUserInteraction> _mockUserInteraction;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;
        private Mock<IMainService> _mockMainService;
        private Mock<ILatestSafetyCheckRepository> _mockLatestSafetyCheckRepository;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _latestSafetyCheck = _fixture.Create<LatestSafetyCheck>();
            _mobileData = _fixture.Create<MobileData>();

            _mockMainService = _fixture.InjectNewMock<IMainService>();
            _mockMainService.Setup(m => m.CurrentMobileData).Returns(_mobileData);
            _mockMainService.Setup(m => m.CurrentDriver).Returns(_fixture.Create<Driver>());

            _mockLatestSafetyCheckRepository = _fixture.InjectNewMock<ILatestSafetyCheckRepository>();
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            _mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(_mockUserInteraction.Object);

            _mockUserInteraction.ConfirmReturnsTrue();
            _mockUserInteraction.AlertInvokeAction();

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());
        }

        #endregion Setup

        #region Tests

        [Fact]
        public void DisplaySafetyCheckVM_VehicleRegistration_NotNull()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Vehicle: " + _latestSafetyCheck.VehicleSafetyCheck.VehicleRegistration, displaySafetyCheckVM.VehicleRegistration);
        }

        [Fact]
        public void DisplaySafetyCheckVM_VehicleRegistration_Null()
        {
            base.ClearAll();

            _latestSafetyCheck.VehicleSafetyCheck = null;
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Vehicle: " , displaySafetyCheckVM.VehicleRegistration);
        }

        [Fact]
        public void DisplaySafetyCheckVM_TrailerRegistration_NotNull()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Trailer: " + _latestSafetyCheck.TrailerSafetyCheck.VehicleRegistration, displaySafetyCheckVM.TrailerRegistration);
        }

        [Fact]
        public void DisplaySafetyCheckVM_TrailerRegistration_Null()
        {
            base.ClearAll();

            _latestSafetyCheck.TrailerSafetyCheck = null;
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Trailer: ", displaySafetyCheckVM.TrailerRegistration);
        }

        [Fact]
        public void DisplaySafetyCheckVM_VehicleSafetyCheckStatus_NotNull()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Checked: " + _latestSafetyCheck.VehicleSafetyCheck.EffectiveDate.ToString("g"), displaySafetyCheckVM.VehicleSafetyCheckStatus);
        }

        [Fact]
        public void DisplaySafetyCheckVM_VehicleSafetyCheckStatus_Null()
        {
            base.ClearAll();

            _latestSafetyCheck.VehicleSafetyCheck = null;
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Checked: ", displaySafetyCheckVM.VehicleSafetyCheckStatus);
        }

        [Fact]
        public void DisplaySafetyCheckVM_TrailerSafetyCheckStatus_NotNull()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Checked: " + _latestSafetyCheck.TrailerSafetyCheck.EffectiveDate.ToString("g"), displaySafetyCheckVM.TrailerSafetyCheckStatus);
        }

        [Fact]
        public void DisplaySafetyCheckVM_TrailerSafetyCheckStatus_Null()
        {
            base.ClearAll();

            _latestSafetyCheck.TrailerSafetyCheck = null;
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            Assert.Equal("Checked: ", displaySafetyCheckVM.TrailerSafetyCheckStatus);
        }


        [Fact]
        public void DisplaySafetyCheckVM_CreationOfSafetyCheckFaultItemViewModels()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Build<DisplaySafetyCheckViewModel>().Without(dscvm => dscvm.SafetyCheckFaultItemViewModels).Create<DisplaySafetyCheckViewModel>();

            int faultsCount = _latestSafetyCheck.TrailerSafetyCheck.Faults.Count() + _latestSafetyCheck.VehicleSafetyCheck.Faults.Count();

            Assert.Equal(faultsCount, displaySafetyCheckVM.SafetyCheckFaultItemViewModels.Count());

            int vehicleCount = 0;
            int trailerCount = 0;

            foreach (var safetyCheckFaultVM in displaySafetyCheckVM.SafetyCheckFaultItemViewModels)
            {
                if (safetyCheckFaultVM.FaultType == "VEH")
                    vehicleCount++;
                else if (safetyCheckFaultVM.FaultType == "TRL")
                    trailerCount++;
            }

            Assert.Equal(3, vehicleCount);
            Assert.Equal(3, trailerCount);
            
        }

        /// <summary>
        /// This test is to make sure the pop up only appears when a comment has been entered.
        /// </summary>
        [Fact]
        public void DisplaySafetyCheckVM_SafetyCheckCommentPopUpWithComment()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            var fault = _fixture.Create<DisplaySafetyCheckFaultItemViewModel>();

            displaySafetyCheckVM.ShowSafetyCheckFaultCommand.Execute(fault);

            _mockUserInteraction.Verify(mui => mui.Alert(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// This test is to make sure the pop up doesn't appears when a comment has not  been entered.
        /// </summary>
        [Fact]
        public void DisplaySafetyCheckVM_SafetyCheckCommentPopUpWithoutComment()
        {
            base.ClearAll();

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            var fault = _fixture.Create<DisplaySafetyCheckFaultItemViewModel>();
            fault.FaultCheckComment = string.Empty;

            displaySafetyCheckVM.ShowSafetyCheckFaultCommand.Execute(fault);

            _mockUserInteraction.Verify(mui => mui.Alert(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void DisplaySafetyCheckVM_NullSafetyChecks()
        {
            base.ClearAll();

            _latestSafetyCheck.VehicleSafetyCheck = null;
            _latestSafetyCheck.TrailerSafetyCheck = null;
            _mockLatestSafetyCheckRepository.Setup(mls => mls.GetForDriver(It.IsAny<Guid>())).Returns(_latestSafetyCheck);

            _mockMainService.Setup(mms => mms.OnManifestPage).Returns(true);

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            _mockUserInteraction.Verify(mui => mui.Alert(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockNavigationService.Verify(ns => ns.MoveToNext(It.Is<NavItem<MobileData>>(ni => ni.ID == Guid.Empty)), Times.Once);
        }

        [Fact]
        public void DisplaySafetyCheckVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockCustomUserInteraction.Setup(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            displaySafetyCheckVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockNavigationService.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public void DisplaySafetyCheckVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();


            var displaySafetyCheckVM = _fixture.Create<DisplaySafetyCheckViewModel>();

            displaySafetyCheckVM.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockCustomUserInteraction.Verify(cui => cui.PopUpCurrentInstructionNotifaction(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);


        }

        #endregion Tests
    }
}
