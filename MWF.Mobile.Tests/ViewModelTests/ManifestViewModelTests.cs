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
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class ManifestViewModelTests
        : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private MobileData _mobileData;
        private StartupService _startupService;
        private Mock<IMobileDataRepository> _mobileDataRepoMock;
        private Mock<IApplicationProfileRepository> _mockApplicationProfile;
        private Mock<IMvxMessenger> _mockMessenger;
        

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";

            _startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(_startupService);

            _mockApplicationProfile = _fixture.InjectNewMock<IApplicationProfileRepository>();
            List<ApplicationProfile> appProfiles = new List<ApplicationProfile>();
            ApplicationProfile appProfile = new ApplicationProfile();
            appProfile.DisplayRetention = 2;
            appProfile.DisplaySpan = 2;
            appProfiles.Add(appProfile);

            _mockApplicationProfile.Setup(map => map.GetAll()).Returns(appProfiles);

            _mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());
        }

        /// <summary>
        ///  Tests that the Manifest creates successfully
        /// </summary>
        [Fact]
        public void ManifestVM_SuccessfulCreationOfInstruction()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now;
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal((mobileDataStartedList.Count + mobileDataNotStartedList.Count), viewModel.InstructionsCount);
            
            //check that the logged indriver id was used for the calls to the mobile data repository
            _mobileDataRepoMock.Verify(mdr => mdr.GetInProgressInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Once);
            _mobileDataRepoMock.Verify(mdr => mdr.GetNotStartedInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Once);

        }

        /// <summary>
        /// Tests that the refresh function on the manifest instruction list works.
        /// </summary>
        [Fact]
        public void ManifestVM_SuccessfulInstructionRefresh()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now.AddDays(-4);
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            viewModel.RefreshListCommand.Execute(null);

            _mobileDataRepoMock.Verify(mdr => mdr.GetInProgressInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Exactly(2));
            _mobileDataRepoMock.Verify(mdr => mdr.GetNotStartedInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Exactly(2));

        }

        /// <summary>
        /// Tests that the instruction count is correct and doesn't count the dummy mobile data.
        /// </summary>
        [Fact]
        public void ManifestVM_CorrectInstructionCount()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now;
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal((mobileDataStartedList.Count + mobileDataNotStartedList.Count), viewModel.InstructionsCount);
        }

        /// <summary>
        /// This test make sure instructions are excluded when outside of the display retention
        /// </summary>
        [Fact]
        public void ManifestVM_InstructionDisplayRetention_Exclude()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now.AddDays(-4);
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal(0, viewModel.InstructionsCount);
        }

        /// <summary>
        /// This test make sure instructions are included within the display retention
        /// </summary>
        [Fact]
        public void ManifestVM_InstructionDisplayRetention_Include()
        {
            base.ClearAll();

            List<ApplicationProfile> appProfiles = new List<ApplicationProfile>();
            ApplicationProfile appProfile = new ApplicationProfile();
            appProfile.DisplayRetention = 2;
            appProfile.DisplaySpan = 2;
            appProfiles.Add(appProfile);

            _mockApplicationProfile.Setup(map => map.GetAll()).Returns(appProfiles);

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now.AddDays(-1);
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal((mobileDataStartedList.Count + mobileDataNotStartedList.Count), viewModel.InstructionsCount);
        }

        /// <summary>
        /// This test make sure instructions are excluded when outside of the display span
        /// </summary>
        [Fact]
        public void ManifestVM_InstructionDisplaySpan_Exclude()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now.AddDays(4);
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal(0, viewModel.InstructionsCount);
        }

        /// <summary>
        /// This test make sure instructions are included within the display span
        /// </summary>
        [Fact]
        public void ManifestVM_InstructionDisplaySpan_Include()
        {
            base.ClearAll();

            List<ApplicationProfile> appProfiles = new List<ApplicationProfile>();
            ApplicationProfile appProfile = new ApplicationProfile();
            appProfile.DisplayRetention = 2;
            appProfile.DisplaySpan = 2;
            appProfiles.Add(appProfile);

            _mockApplicationProfile.Setup(map => map.GetAll()).Returns(appProfiles);

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            var mobileData = _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileData.EffectiveDate = DateTime.Now.AddDays(1);
            mobileDataStartedList.Add(mobileData);

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal((mobileDataStartedList.Count + mobileDataNotStartedList.Count), viewModel.InstructionsCount);
        }


        /// <summary>
        /// Tests that the toast message appears when there is no internet connection
        /// on the refresh of the manifest instruction list.
        /// </summary>
        [Fact]
        public void ManifestVM_NoInternetShowToast()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == false));

            var toast = new Mock<IToast>();
            _fixture.Inject<IToast>(toast.Object);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            viewModel.RefreshListCommand.Execute(null);

            toast.Verify(t => t.Show("No internet connection!"));

        }

        /// <summary>
        /// Tests that the "No Active Instructions" Item displays when no items are in the active instruction section.
        /// </summary>
        [Fact]
        public void ManifestVM_ShowNoInstructionItem_ActiveInstruction()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal("No Active Instructions", viewModel.Sections.First().First().PointDescripion);
        }

        /// <summary>
        /// Tests that the "No Instructions" Item displays when no items are in the instruction section.
        /// </summary>
        [Fact]
        public void ManifestVM_ShowNoInstructionItem_NonActiveInstruction()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            _mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            _mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal("No Instructions", viewModel.Sections.ElementAt(1).First().PointDescripion);
        }

        [Fact]
        public void ManifestVM_CheckInstructionNotification()
        {

            base.ClearAll();

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            _mobileData.GroupTitle = "UpdateTitle";

            viewModel.CheckInstructionNotification(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            //It is checked twice because it checks when it creates the view model and when you refresh.
            _mobileDataRepoMock.Verify(mdr => mdr.GetInProgressInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Exactly(2));
            _mobileDataRepoMock.Verify(mdr => mdr.GetNotStartedInstructions(It.Is<Guid>(i => i == _startupService.LoggedInDriver.ID)), Times.Exactly(2));

        }

        [Fact]
        public void ManifestVM_IsVisible()
        {
            base.ClearAll();

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            viewModel.IsVisible(false);

            _mockMessenger.Verify(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()), Times.Once);
        }
    }
}
