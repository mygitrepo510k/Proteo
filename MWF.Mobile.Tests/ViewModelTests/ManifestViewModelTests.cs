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

        [Fact]
        public void ManifestVM_SuccessfulCreationOfInstruction()
        {
            base.ClearAll();

            List<MobileData> mobileDataStartedList = new List<MobileData>();
            mobileDataStartedList.Add( _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.OnSite })); 

            List<MobileData> mobileDataNotStartedList = new List<MobileData>();
            mobileDataNotStartedList.Add( _fixture.Create<MobileData>(new MobileData() { ProgressState = Core.Enums.InstructionProgress.NotStarted }));
           
            var mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();
            mobileDataRepoMock.Setup(mdr => mdr.GetInProgressInstructions()).Returns(mobileDataStartedList);
            mobileDataRepoMock.Setup(mdr => mdr.GetNotStartedInstructions()).Returns(mobileDataNotStartedList);

            var viewModel = _fixture.Build<ManifestViewModel>().Without(mvm => mvm.Sections).Create<ManifestViewModel>();

            Assert.Equal(viewModel.InstructionsCount, (mobileDataNotStartedList.Count+mobileDataStartedList.Count));

        }
    }
}
