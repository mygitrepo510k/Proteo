using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Services;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using Xunit;
using MWF.Mobile.Core.Repositories.Interfaces;

namespace MWF.Mobile.Tests.RepositoryTests
{
    public class MobileApplicationDataRepositoryTests
        : MvxIoCSupportingTest
    {
        private Mock<ISQLiteConnection> _connectionMock;
        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _connectionMock = new Mock<ISQLiteConnection>();
            _connectionMock.Setup(c => c.RunInTransaction(It.IsAny<Action>())).Callback((Action a) => a.Invoke());

            var dataServiceMock = Mock.Of<IDataService>(ds => ds.Connection == _connectionMock.Object);
            _fixture.Register<IDataService>(() => dataServiceMock);
        }

        [Fact]
        public void Repository_Returns_Inprogress_Instructions()
        {
            base.ClearAll();
            var mobileApplicationDataRepository = new Mock<IMobileDataRepository>(); //_fixture.Create<MobileDataRepository>();
            var mobileApplicationData = _fixture.CreateMany<MobileData>();
            
            mobileApplicationDataRepository.Object.Insert(mobileApplicationData);

            foreach(var instruction in mobileApplicationDataRepository.Object.GetInProgressInstructions())
            {
                Assert.True((instruction.ProgressState == Core.Enums.InstructionProgress.OnSite || instruction.ProgressState == Core.Enums.InstructionProgress.Driving), "An instruction has been returned that has not been started");
            }
        }
    }
}