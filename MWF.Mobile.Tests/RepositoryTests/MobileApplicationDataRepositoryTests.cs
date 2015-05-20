using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Services;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Linq;
using Xunit;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using System.Collections.Generic;
using System.Collections;
using MWF.Mobile.Tests.Helpers;

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

            var dataServiceMock = Mock.Of<IDataService>(ds => ds.GetDBConnection() == _connectionMock.Object);
            _fixture.Register<IDataService>(() => dataServiceMock);
        }

        [Fact]
        public void Repository_Returns_Inprogress_Instructions()
        {
            base.ClearAll();

            List<MobileData> mobileDataList = new List<MobileData>();
            mobileDataList.Add(new MobileData { ID = new Guid(), ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileDataList.Add(new MobileData { ID = new Guid(), ProgressState = Core.Enums.InstructionProgress.NotStarted });

            MockITableQuery<MobileData> mockTableQuery = new MockITableQuery<MobileData>();
            mockTableQuery.Items = mobileDataList;

            _connectionMock.Setup(c => c.Table<MobileData>()).Returns(mockTableQuery);

            var mdr = _fixture.Create<MobileDataRepository>();

            var inProgressInstructions = mdr.GetInProgressInstructions(Guid.NewGuid()).ToList();

            foreach (var instruction in inProgressInstructions)
            {
                Assert.True((instruction.ProgressState == Core.Enums.InstructionProgress.Driving || instruction.ProgressState == Core.Enums.InstructionProgress.OnSite), "An instruction has been returned that has already started");
            }            
        }

        [Fact]
        public void Repository_Returns_NotStarted_Instructions()
        {
            base.ClearAll();

            List<MobileData> mobileDataList = new List<MobileData>();
            mobileDataList.Add(new MobileData { ID = new Guid(), ProgressState = Core.Enums.InstructionProgress.OnSite });
            mobileDataList.Add(new MobileData { ID = new Guid(), ProgressState = Core.Enums.InstructionProgress.NotStarted });

            MockITableQuery<MobileData> mockTableQuery = new MockITableQuery<MobileData>();
            mockTableQuery.Items = mobileDataList;

            _connectionMock.Setup(c => c.Table<MobileData>()).Returns(mockTableQuery);

            var mdr = _fixture.Create<MobileDataRepository>();

            var notStartedInstructions = mdr.GetNotStartedInstructions(Guid.NewGuid()).ToList();

            foreach (var instruction in notStartedInstructions)
            {
                Assert.True((instruction.ProgressState == Core.Enums.InstructionProgress.NotStarted ), "An instruction has been returned that has already started");
            }
        }
    } 
}
