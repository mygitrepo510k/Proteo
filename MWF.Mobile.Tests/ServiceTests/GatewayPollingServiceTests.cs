﻿using System;
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
        private MvxSubscriptionToken _pollTimerToken;

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
            _pollTimerToken = messenger.Subscribe<Core.Messages.GatewayPollTimerCommandMessage>(m =>
            {
                if (m.Command == Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Trigger)
                    messenger.Publish(new Core.Messages.GatewayPollTimerElapsedMessage(this));
            });
        }

        [Fact]
        public async Task GatewayPollingService_AddsSingleInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Add, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(2000);

            // Check that we insert the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Insert(It.Is<MobileData>(md => md.ID == id)), Times.Once);
        }

        [Fact]
        public async Task GatewayPollingService_UpdatesSingleInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Update, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

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
        public async Task GatewayPollingService_DeletesSingleInstruction()
        {
            base.ClearAll();
            var id = new Guid();
            CreateSingleMobileData(SyncState.Delete, id);

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check we look for the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(g => g.ToString() == id.ToString())), Times.Once);
            // Check we delete the instruction
            _mockMobileDataRepo.Verify(mdr => mdr.Delete(It.Is<MobileData>(md => md.ID == id)), Times.Once);
        }

        [Fact]
        public async Task GatewayPollingService_AddsMultipleInstructions()
        {
            base.ClearAll();
            
            var id1 = new Guid();
            var id2 = new Guid();
            var id3 = new Guid();
            Guid[] ids = {id1, id2, id3};
            CreateMultipleMobileDatas(SyncState.Add, ids);

            List<MobileData> insertList = new List<MobileData>();

            _mockMobileDataRepo.Setup(mdr => mdr.Insert(It.IsAny<MobileData>()))
                               .Callback<MobileData>((md) => { insertList.Add(md); });

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check that we insert the instructions
            var counter = 0;
            foreach (var item in insertList)
            {
                Assert.Equal(item.ID, ids[counter]);
                counter++;
            }
        }

        [Fact]
        public async Task GatewayPollingService_UpdatesMultipleInstructions()
        {
            base.ClearAll();

            var id1 = new Guid();
            var id2 = new Guid();
            var id3 = new Guid();
            Guid[] ids = { id1, id2, id3 };
            CreateMultipleMobileDatas(SyncState.Add, ids);

            List<Guid> getList = new List<Guid>();
            List<MobileData> deleteList = new List<MobileData>();
            List<MobileData> insertList = new List<MobileData>();

            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.IsAny<Guid>()))
                               .Callback<Guid>((md) => { getList.Add(md); });

            _mockMobileDataRepo.Setup(mdr => mdr.Delete(It.IsAny<MobileData>()))
                               .Callback<MobileData>((md) => { deleteList.Add(md); });

            _mockMobileDataRepo.Setup(mdr => mdr.Insert(It.IsAny<MobileData>()))
                               .Callback<MobileData>((md) => { insertList.Add(md); });

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check that we get the instructions
            var getCounter = 0;
            foreach (var item in getList)
            {
                Assert.Equal(item, ids[getCounter]);
                getCounter++;
            }

            // Check that we delete the instructions
            var deleteCounter = 0;
            foreach (var item in deleteList)
            {
                Assert.Equal(item.ID, ids[deleteCounter]);
                deleteCounter++;
            }

            // Check that we insert the instructions
            var insertCounter = 0;
            foreach (var item in insertList)
            {
                Assert.Equal(item.ID, ids[insertCounter]);
                insertCounter++;
            }
        }

        [Fact]
        public async Task GatewayPollingService_DeletesMultipleInstructions()
        {
            base.ClearAll();

            var id1 = new Guid();
            var id2 = new Guid();
            var id3 = new Guid();
            Guid[] ids = { id1, id2, id3 };
            CreateMultipleMobileDatas(SyncState.Add, ids);

            List<Guid> getList = new List<Guid>();
            List<MobileData> deleteList = new List<MobileData>();

            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.IsAny<Guid>()))
                               .Callback<Guid>((md) => { getList.Add(md); });

            _mockMobileDataRepo.Setup(mdr => mdr.Delete(It.IsAny<MobileData>()))
                               .Callback<MobileData>((md) => { deleteList.Add(md); });

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();

            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(100);

            // Check that we get the instructions
            var getCounter = 0;
            foreach (var item in getList)
            {
                Assert.Equal(item, ids[getCounter]);
                getCounter++;
            }

            // Check that we delete the instructions
            var deleteCounter = 0;
            foreach (var item in deleteList)
            {
                Assert.Equal(item.ID, ids[deleteCounter]);
                deleteCounter++;
            }
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
                counter++;
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