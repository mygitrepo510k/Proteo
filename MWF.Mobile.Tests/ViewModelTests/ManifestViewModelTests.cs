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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class ManifestViewModelTests
        : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

        }

        /// <summary>
        ///  Tests that the Manifest creates successfully
        /// </summary>
        [Fact]
        public void ManifestVM_SuccessfulCreationOfInstruction()
        {
            base.ClearAll();

            StartupService startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startupService);


            List<MobileData> mobileDataStartedList = new List<MobileData>();
            mobileDataStartedList.Add( _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite })); 

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();
           
            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal((mobileDataStartedList.Count + mobileDataNotStartedList.Count), viewModel.InstructionsCount);
            
            //check that the logged indriver id was used for the calls to the mobile data repository
            mobileDataRepoMock.Verify(mdr => mdr.GetInProgressInstructions(It.Is<Guid>(i => i == startupService.LoggedInDriver.ID)), Times.Once);
            mobileDataRepoMock.Verify(mdr => mdr.GetNotStartedInstructions(It.Is<Guid>(i => i == startupService.LoggedInDriver.ID)), Times.Once);

        }

        /// <summary>
        /// Tests that the refresh function on the manifest instruction list works.
        /// </summary>
        [Fact]
        public void ManifestVM_SuccessfulInstructionRefresh()
        {
            base.ClearAll();

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            StartupService startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startupService);

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            mobileDataStartedList.Add(_fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite }));

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            _fixture.Inject<IMobileDataRepository>(mobileDataRepoMock.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            viewModel.RefreshListCommand.Execute(null);

            mobileDataRepoMock.Verify(mdr => mdr.GetInProgressInstructions(It.Is<Guid>(i => i == startupService.LoggedInDriver.ID)), Times.Exactly(2));
            mobileDataRepoMock.Verify(mdr => mdr.GetNotStartedInstructions(It.Is<Guid>(i => i == startupService.LoggedInDriver.ID)), Times.Exactly(2));

        }

        /// <summary>
        /// Tests that the instruction count is correct and doesn't count the dummy mobile data.
        /// </summary>
        [Fact]
        public void ManifestVM_CorrectInstructionCount()
        {
            base.ClearAll();

            StartupService startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startupService);


            List<MobileData> mobileDataStartedList = new List<MobileData>();
            mobileDataStartedList.Add(_fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite }));

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

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

            StartupService startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startupService);


            List<MobileData> mobileDataStartedList = new List<MobileData>();
            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

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

            StartupService startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(startupService);


            List<MobileData> mobileDataStartedList = new List<MobileData>();
            List<MobileData> mobileDataNotStartedList = new List<MobileData>();

            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions(It.IsAny<Guid>())).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions(It.IsAny<Guid>())).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal("No Instructions", viewModel.Sections.ElementAt(1).First().PointDescripion);
        }
    }
}
