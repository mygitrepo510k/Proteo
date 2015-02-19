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
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;
using MWF.Mobile.Core.Portable;

namespace MWF.Mobile.Tests.ServiceTests
{
    public class GatewayPollingServiceTests : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<IGatewayQueuedService> _mockGatewayQueuedService;
        private MvxSubscriptionToken _pollTimerToken;
        private ApplicationProfile _applicationProfile;
        private Mock<IGatewayService> _gatewayMock;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;
        private Mock<IMvxMessenger> _mockMvxMessenger;
        private Mock<IMainService> _mockMainService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            IDeviceRepository deviceRepo = Mock.Of<IDeviceRepository>(dr => dr.GetAll() == _fixture.CreateMany<Device>());
            _mockMobileDataRepo = new Mock<IMobileDataRepository>();
            _fixture.Inject<IMobileDataRepository>(_mockMobileDataRepo.Object);

            _mockGatewayQueuedService = new Mock<IGatewayQueuedService>();
            _fixture.Inject<IGatewayQueuedService>(_mockGatewayQueuedService.Object);

            _applicationProfile = new ApplicationProfile();
            var applicationRepo = _fixture.InjectNewMock<IApplicationProfileRepository>();
            applicationRepo.Setup(ar => ar.GetAll()).Returns(new List<ApplicationProfile>() { _applicationProfile});

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();
            _mockCustomUserInteraction.Setup(cui => cui.PopUpInstructionNotifaction(It.IsAny<List<MobileData>>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<List<MobileData>, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            var mockStartupService = Mock.Of<IStartupService>(ssr => ssr.CurrentVehicle == _fixture.Create<Vehicle>()
                                                    && ssr.LoggedInDriver == _fixture.Create<Driver>());
            _fixture.Inject<IStartupService>(mockStartupService);

            _mockMainService = new Mock<IMainService>();
            _fixture.Inject<IMainService>(_mockMainService.Object);

            IRepositories repos = Mock.Of<IRepositories>(r => r.DeviceRepository == deviceRepo);
            _fixture.Register<IRepositories>(() => repos);

            _mockMvxMessenger = new Mock<IMvxMessenger>();
            _fixture.Inject<IMvxMessenger>(_mockMvxMessenger.Object);

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
        public async Task GatewayPollingService_RespectsApplicationProfile_DataSpan()
        {
            base.ClearAll();

            _applicationProfile.DataSpan = 5;

            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            IEnumerable<MobileData> mobileDatas = new List<MobileData>();

            _gatewayMock = _fixture.InjectNewMock<IGatewayService>();
            _gatewayMock.Setup(g => g.GetDriverInstructions(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                        .Returns(Task.FromResult(mobileDatas))
                        .Callback<string, Guid, DateTime, DateTime>((s1, g, dt1, dt2) => { startDate = dt1; endDate = dt2; });

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected()));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());
            var service = _fixture.Create<GatewayPollingService>();




            service.StartPollingTimer();
            service.PollForInstructions();

            // Allow the timer to process the queue
            await Task.Delay(2000);

            //Check that the start and end date of driver instructions requested from gateway service matches up with the the span 
            //specified int he application profile
            Assert.Equal(_applicationProfile.DataSpan, (endDate - startDate).Days);


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
            // Check that it publishes the updated instruction to the current view model
            _mockMvxMessenger.Verify(mm => mm.Publish(It.Is<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(inm => id.ToString() == inm.InstructionID.ToString() && inm.Command == Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update)), Times.Once);
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
            // Check that it publishes the deleted instruction to the current view model
            _mockMvxMessenger.Verify(mm => mm.Publish(It.Is<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(inm => id.ToString() == inm.InstructionID.ToString() && inm.Command == Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete)), Times.Once);

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
            CreateMultipleMobileDatas(SyncState.Update, ids);

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

            var publishCounter = 0;
            // Check that we get the instructions
            foreach (var item in getList)
            {
                _mockMvxMessenger.Verify(mm => mm.Publish(It.Is<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(inm => inm.InstructionID.ToString() == ids[publishCounter].ToString() && inm.Command == Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update)), Times.Exactly(3));
                publishCounter++;
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
            CreateMultipleMobileDatas(SyncState.Delete, ids);

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

            var publishCounter = 0;
            // Check that we get the instructions
            foreach (var item in getList)
            {
                _mockMvxMessenger.Verify(mm => mm.Publish(It.Is<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(inm => inm.InstructionID.ToString() == ids[publishCounter].ToString() && inm.Command == Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete)), Times.Exactly(3));
                publishCounter++;
            }
        }

        /// <summary>
        /// This tests is to make sure when a instruction is polled then an acknowledgement is sent off.
        /// </summary>
        [Fact]
        public async Task GatewayPollingService_AcknowledgeInstruction()
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

            _mockGatewayQueuedService.Verify(mgqs => 
                mgqs.AddToQueue(It.IsAny<IEnumerable<MWF.Mobile.Core.Models.GatewayServiceRequest.Action<MWF.Mobile.Core.Models.SyncAck>>>()), Times.Once);

            _mockMainService.Verify(mms => mms.SendReadChunk(It.IsAny<IEnumerable<MobileData>>()), Times.Once);

        }

        [Fact]
        public async Task GatewayPollingService_SingleInstructionNotficationPopUp()
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

            _mockCustomUserInteraction.Verify(cui => cui.PopUpInstructionNotifaction(It.Is<List<MobileData>>(lmd => lmd.Count == 1), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockGatewayQueuedService.Verify(mgqs =>
             mgqs.AddToQueue(It.IsAny<IEnumerable<MWF.Mobile.Core.Models.GatewayServiceRequest.Action<MWF.Mobile.Core.Models.SyncAck>>>()), Times.Once);

            _mockMainService.Verify(mms => mms.SendReadChunk(It.IsAny<IEnumerable<MobileData>>()), Times.Once);
        }

        [Fact]
        public async Task GatewayPollingService_MultipleInstructionNotficationPopUp()
        {
            base.ClearAll();

            var id1 = new Guid();
            var id2 = new Guid();
            var id3 = new Guid();
            Guid[] ids = { id1, id2, id3 };
            CreateMultipleMobileDatas(SyncState.Delete, ids);

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

            _mockCustomUserInteraction.Verify(cui => cui.PopUpInstructionNotifaction(It.Is<List<MobileData>>(lmd => lmd.Count == ids.Count()), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _mockGatewayQueuedService.Verify(mgqs =>
             mgqs.AddToQueue(It.IsAny<IEnumerable<MWF.Mobile.Core.Models.GatewayServiceRequest.Action<MWF.Mobile.Core.Models.SyncAck>>>()), Times.Once);

            _mockMainService.Verify(mms => mms.SendReadChunk(It.IsAny<IEnumerable<MobileData>>()), Times.Once);

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

            _gatewayMock = new Mock<IGatewayService>();
            _gatewayMock.Setup(
                gm =>
                gm.GetDriverInstructions(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>())).Returns(Task.FromResult(mobileDatas));

            _fixture.Inject(_gatewayMock.Object);
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

            _gatewayMock = new Mock<IGatewayService>();
            _gatewayMock.Setup(
                gm =>
                gm.GetDriverInstructions(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>())).Returns(Task.FromResult(mobileDatas));

            _fixture.Inject(_gatewayMock.Object);
        }

        #endregion
    }
}
