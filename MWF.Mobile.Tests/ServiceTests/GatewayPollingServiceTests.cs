using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{
    public class GatewayPollingServiceTests : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            IDeviceRepository deviceRepo = Mock.Of<IDeviceRepository>(dr => dr.GetAll() == _fixture.CreateMany<Device>());
            _mockMobileDataRepo = new Mock<IMobileDataRepository>();
            _fixture.Inject<IMobileDataRepository>(_mockMobileDataRepo.Object);

            var mockStartupService = Mock.Of<IStartupService>(ssr => ssr.CurrentVehicle == _fixture.Create<Vehicle>()
                                                    && ssr.LoggedInDriver == _fixture.Create<Driver>());
            _fixture.Inject<IStartupService>(mockStartupService);

            IRepositories repos = Mock.Of<IRepositories>(r => r.DeviceRepository == deviceRepo);
            _fixture.Register<IRepositories>(() => repos);

            var messenger = new MvxMessengerHub();
            _fixture.Register<IMvxMessenger>(() => messenger);

            // We don't have the GatewayQueueTimerService so replicate the trigger -> publish elapsed message functionality
            var token = messenger.Subscribe<Core.Messages.GatewayPollTimerCommandMessage>(m =>
            {
                if (m.Command == Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Trigger)
                    messenger.Publish(new Core.Messages.GatewayPollTimerElapsedMessage(this));
            });
        }

        [Fact]
        public async Task GatewayPollingService_AddsInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Add, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();


            //service.StartPollingTimer();
            service.PollForInstructionsAsync();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check that we insert the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Insert(It.Is<MobileData>(md => md.ID == id)), Times.Once);
        }

        [Fact]
        public async Task GatewayPollingService_UpdatesInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Update, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();


            //service.StartPollingTimer();
            service.PollForInstructionsAsync();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check we look for the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(g => g.ToString() == id.ToString())), Times.Once);
            // Check we delete the old instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Delete(It.Is<MobileData>(md => md.ID == id)), Times.Once);
            // Check that we insert the new instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Insert(It.Is<MobileData>(md => md.ID == id)), Times.Once);
        }

        [Fact]
        public async Task GatewayPollingService_DeletesInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Delete, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();


            //service.StartPollingTimer();
            service.PollForInstructionsAsync();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check we look for the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(g => g.ToString() == id.ToString())), Times.Once);
            // Check we delete the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Delete(It.Is<MobileData>(md => md.ID == id)), Times.Once);
        }

        #region Helpers

        private void CreateSingleMobileData(SyncState syncState, Guid id)
        {
            var mobileData = _fixture.Create<MobileData>();
            mobileData.ID = id;
            mobileData.SyncState = syncState;

            if (syncState == SyncState.Update || syncState == SyncState.Delete)
            {
                _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(g => g == id))).Returns(mobileData);
            }

            IEnumerable<MobileData> mobileDatas = new List<MobileData>
                {
                    mobileData
                };

            var gatewayMock = new Mock<IGatewayService>();
            gatewayMock.Setup(
                gm =>
                gm.GetDriverInstructions(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>())).Returns(Task.FromResult(mobileDatas));

            _fixture.Inject(gatewayMock.Object);
        }

        private void CreateMultipleMobileDatas(SyncState syncState, Guid[] ids)
        {
            var mobileDatas = _fixture.CreateMany<MobileData>();
            var counter = 0;
            foreach (var mobileData in mobileDatas)
            {
                mobileData.ID = ids[counter];
                mobileData.SyncState = syncState;
                counter ++;
            }

            var gatewayMock = new Mock<IGatewayService>();
            gatewayMock.Setup(
                gm =>
                gm.GetDriverInstructions(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>())).Returns(Task.FromResult(mobileDatas));

            _fixture.Inject(gatewayMock.Object);
        }

        #endregion
    }
}
