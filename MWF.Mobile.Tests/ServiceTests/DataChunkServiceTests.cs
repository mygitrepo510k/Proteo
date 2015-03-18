using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MWF.Mobile.Core.Models.GatewayServiceRequest;

namespace MWF.Mobile.Tests.ServiceTests
{
    public class DataChunkServiceTests
         : MvxIoCSupportingTest
    {

        #region Private Members

        private IFixture _fixture;
        private Mock<IGatewayQueuedService> _mockGatewayQueuedService;
        private Mock<IMainService> _mockMainService;
             
        private MobileApplicationDataChunkCollection _mobileDataChunkCollection;
        private UploadCameraImageObject _uploadImageObject;
        

        #endregion Private Members


        #region Setup

        protected override void AdditionalSetup()
        {

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            IDriverRepository driverRepo = Mock.Of<IDriverRepository>(dr => dr.GetByID(It.IsAny<Guid>()) == _fixture.Create<Driver>());
            IVehicleRepository vehicleRepo = Mock.Of<IVehicleRepository>(vr => vr.GetByID(It.IsAny<Guid>()) == _fixture.Create<Vehicle>());
            IMobileDataRepository mobileDataRepo = Mock.Of<IMobileDataRepository>(mdr => mdr.GetByID(It.IsAny<Guid>()) == _fixture.Create<MobileData>());

            var mockGpsService = _fixture.InjectNewMock<IGpsService>();
            mockGpsService.Setup(mgps => mgps.GetSmpData(MWF.Mobile.Core.Enums.ReportReason.Begin)).Returns("SMP-BEGIN");
            mockGpsService.Setup(mgps => mgps.GetSmpData(MWF.Mobile.Core.Enums.ReportReason.Drive)).Returns("SMP-DRIVE");
            mockGpsService.Setup(mgps => mgps.GetSmpData(MWF.Mobile.Core.Enums.ReportReason.OnSite)).Returns("SMP-ONSITE");
            mockGpsService.Setup(mgps => mgps.GetSmpData(MWF.Mobile.Core.Enums.ReportReason.Complete)).Returns("SMP-COMPLETE");

            IRepositories repos = Mock.Of<IRepositories>(r => r.DriverRepository == driverRepo && r.VehicleRepository == vehicleRepo && r.MobileDataRepository == mobileDataRepo);
            _fixture.Register<IRepositories>(() => repos);

            _mockGatewayQueuedService = new Mock<IGatewayQueuedService>();
            _mockGatewayQueuedService.Setup(mgqs => mgqs.AddToQueue("fwSyncChunkToServer", It.IsAny<MobileApplicationDataChunkCollection>(), null)).Callback<string, MobileApplicationDataChunkCollection, Parameter[]>((s, m, p) => { _mobileDataChunkCollection = m; });
            _mockGatewayQueuedService.Setup(mgqs => mgqs.AddToQueue("fwSyncPhotos", It.IsAny<UploadCameraImageObject>(), null)).Callback<string, UploadCameraImageObject, Parameter[]>((s, uo, p) => { _uploadImageObject = uo; });

            _fixture.Inject<IGatewayQueuedService>(_mockGatewayQueuedService.Object);

            _mockMainService = _fixture.InjectNewMock<IMainService>();
        }

        #endregion Setup


        #region Tests

        /// <summary>
        /// This test is to verify that the right content is added to the gatewayqueuedservice for a drive chunk
        /// </summary>
        [Fact]
         public void MainService_SendDriveChunk()
        {
            base.ClearAll();

            MobileData mobileData = _fixture.Create<MobileData>();
            mobileData.ProgressState = Core.Enums.InstructionProgress.Driving;

            Driver driver = _fixture.Create<Driver>();

            Vehicle vehicle = _fixture.Create<Vehicle>();

            var dataChunkService = _fixture.Create<DataChunkService>();

            dataChunkService.SendDataChunk(new MobileApplicationDataChunkContentActivity(), mobileData, driver, vehicle, false);     

            _mockGatewayQueuedService.Verify(mgqs =>
                mgqs.AddToQueue("fwSyncChunkToServer", It.IsAny<MobileApplicationDataChunkCollection>(), null), Times.Once);


            MobileApplicationDataChunk mobileDataChunk = _mobileDataChunkCollection.MobileApplicationDataChunkCollectionObject.FirstOrDefault();
            var dataChunkActivities = mobileDataChunk.Data.MobileApplicationDataChunkContentOrderActivities.FirstOrDefault().MobileApplicationDataChunkContentActivitiesObject.FirstOrDefault();

            Assert.Equal("DRIVE", mobileDataChunk.Title);
            Assert.Equal("DRIVE", dataChunkActivities.Title);
            Assert.Equal("SMP-DRIVE", dataChunkActivities.Smp);

        }

        /// <summary>
        /// This test is to verify that the right content is added to the gatewayqueuedservice for a on site chunk
        /// </summary>
        [Fact]
        public void MainService_SendOnSiteChunk()
        {
            base.ClearAll();

            MobileData mobileData = _fixture.Create<MobileData>();
            mobileData.ProgressState = Core.Enums.InstructionProgress.OnSite;

            Driver driver = _fixture.Create<Driver>();

            Vehicle vehicle = _fixture.Create<Vehicle>();

            var dataChunkService = _fixture.Create<DataChunkService>();
            var dataChunk = _fixture.Create<MobileApplicationDataChunkContentActivity>();

            dataChunkService.SendDataChunk(dataChunk, mobileData, driver, vehicle, false);

            _mockGatewayQueuedService.Verify(mgqs =>
                mgqs.AddToQueue("fwSyncChunkToServer", It.IsAny<MobileApplicationDataChunkCollection>(), null), Times.Once);


            MobileApplicationDataChunk mobileDataChunk = _mobileDataChunkCollection.MobileApplicationDataChunkCollectionObject.FirstOrDefault();
            var dataChunkActivities = mobileDataChunk.Data.MobileApplicationDataChunkContentOrderActivities.FirstOrDefault().MobileApplicationDataChunkContentActivitiesObject.FirstOrDefault();

            Assert.Equal("ONSITE", mobileDataChunk.Title);
            Assert.Equal("ONSITE", dataChunkActivities.Title);
            Assert.Equal("SMP-ONSITE", dataChunkActivities.Smp);

        }

        /// <summary>
        /// This test is to verify that the right content is added to the gatewayqueuedservice for a complete chunk
        /// </summary>
        [Fact]
        public void MainService_SendCompleteChunk()
        {
            base.ClearAll();

            MobileData mobileData = _fixture.Create<MobileData>();
            mobileData.ProgressState = Core.Enums.InstructionProgress.Complete;

            Driver driver = _fixture.Create<Driver>();

            Vehicle vehicle = _fixture.Create<Vehicle>();

            var dataChunkService = _fixture.Create<DataChunkService>();
            var dataChunk = _fixture.Create<MobileApplicationDataChunkContentActivity>();

            dataChunkService.SendDataChunk(dataChunk, mobileData, driver, vehicle, false);

            _mockGatewayQueuedService.Verify(mgqs =>
                mgqs.AddToQueue("fwSyncChunkToServer", It.IsAny<MobileApplicationDataChunkCollection>(), null), Times.Once);


            MobileApplicationDataChunk mobileDataChunk = _mobileDataChunkCollection.MobileApplicationDataChunkCollectionObject.FirstOrDefault();
            var dataChunkActivities = mobileDataChunk.Data.MobileApplicationDataChunkContentOrderActivities.FirstOrDefault().MobileApplicationDataChunkContentActivitiesObject.FirstOrDefault();

            Assert.Equal("COMPLETE", mobileDataChunk.Title);
            Assert.Equal("COMPLETE", dataChunkActivities.Title);
            Assert.Equal("SMP-COMPLETE", dataChunkActivities.Smp);

        }

        /// <summary>
        /// This test is to verify that the right content is added to the gatewayqueuedservice for a Read chunk
        /// </summary>
        [Fact]
        public void MainService_SendReadChunk()
        {
            base.ClearAll();


            Driver driver = _fixture.Create<Driver>();

            Vehicle vehicle = _fixture.Create<Vehicle>();

            var dataChunkService = _fixture.Create<DataChunkService>();


            var instructions = _fixture.CreateMany<MobileData>();

            dataChunkService.SendReadChunk(instructions, driver, vehicle);

            _mockGatewayQueuedService.Verify(mgqs =>
                mgqs.AddToQueue("fwSyncChunkToServer", It.IsAny<MobileApplicationDataChunkCollection>(), null), Times.Once);


            MobileApplicationDataChunk mobileDataChunk = _mobileDataChunkCollection.MobileApplicationDataChunkCollectionObject.FirstOrDefault();
            var dataChunkActivities = mobileDataChunk.Data.MobileApplicationDataChunkContentOrderActivities.FirstOrDefault().MobileApplicationDataChunkContentActivitiesObject.FirstOrDefault();

            Assert.Equal("READ", mobileDataChunk.Title);
            Assert.Equal("READ", dataChunkActivities.Title);
            Assert.Equal("SMP-BEGIN", dataChunkActivities.Smp);

            Assert.Equal(3, _mobileDataChunkCollection.MobileApplicationDataChunkCollectionObject.Count);

        }

        #endregion Tests
    }
}
