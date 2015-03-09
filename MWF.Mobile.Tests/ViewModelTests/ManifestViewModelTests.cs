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
using MWF.Mobile.Tests.Helpers;
using MWF.Mobile.Core.Repositories.Interfaces;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class ManifestViewModelTests
        : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private MobileData _mobileData;
        private StartupService _startupService;
        private Mock<IMobileDataRepository> _mobileDataRepoMock;
        

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(_startupService);

            _mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());
        }

        /// <summary>
        ///  Tests that the Manifest creates successfully
        /// </summary>
        [Fact]
        public void ManifestVM_SuccessfulCreationOfInstruction()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            mobileDataStartedList.Add( _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite })); 

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
            mobileDataStartedList.Add(_fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite }));

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
            mobileDataStartedList.Add(_fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite }));

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
    }
}
