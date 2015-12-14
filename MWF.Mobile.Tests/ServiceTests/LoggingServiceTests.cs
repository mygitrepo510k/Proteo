using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{
    public class LoggingServiceTests
        : MvxIoCSupportingTest
    {

        #region Private Members

        private IFixture _fixture;
        private Mock<IGatewayService> _mockGatewayService;
        private Mock<ILogMessageRepository> _mockLogMessageRepo;
        private Mock<IDeviceInfo> _mockDeviceInfo;
        private Mock<ICustomUserInteraction> _mockUserInteraction;

        #endregion Private Members


        #region Setup

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _mockDeviceInfo = _fixture.InjectNewMock<IDeviceInfo>();
            _mockDeviceInfo.Setup(mdi => mdi.GetDeviceIdentifier()).Returns("TestID");

            _mockLogMessageRepo = new Mock<ILogMessageRepository>();
            _fixture.Inject<ILogMessageRepository>(_mockLogMessageRepo.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.Create<IMvxMessenger>());

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _mockGatewayService = _fixture.InjectNewMock<IGatewayService>();
        }

        #endregion Setup


        [Fact]
        public void LoggingService_LogInfoEvent()
        {
            base.ClearAll();

            var loggingService = _fixture.Create<LoggingService>();

            loggingService.LogEventAsync("Test Log", Core.Enums.LogType.Info);

            _mockLogMessageRepo.Verify(mlm => mlm.Insert(It.Is<LogMessage>(lm => lm.LogType == Core.Enums.LogType.Info)), Times.Once);
        }

        [Fact]
        public void LoggingService_LogExceptions()
        {
            base.ClearAll();

            var loggingService = _fixture.Create<LoggingService>();

            loggingService.LogEventAsync(new Exception("Test Exception"));

            _mockLogMessageRepo.Verify(mlm => mlm.Insert(It.Is<LogMessage>(lm => lm.LogType == Core.Enums.LogType.Error)), Times.Once);
        }

        [Fact]
        public async Task LoggingService_UploadEventsSuccessfully()
        {
            base.ClearAll();

            _mockLogMessageRepo.Setup(mlm => mlm.GetAllAsync()).ReturnsAsync(_fixture.CreateMany<LogMessage>());

            HttpResult result = new HttpResult();
            result.StatusCode = System.Net.HttpStatusCode.OK;

            _mockGatewayService.Setup(mgs => mgs.PostLogMessageAsync(It.IsAny<DeviceLogMessage>())).Returns(Task.FromResult<HttpResult>(result));

            var loggingService = _fixture.Create<LoggingService>();

            await loggingService.UploadLoggedEventsAsync();

            _mockLogMessageRepo.Verify(mlm => mlm.Delete(It.IsAny<LogMessage>()), Times.Exactly(3));

        }

        [Fact]
        public async Task LoggingService_UploadEventsError()
        {
            base.ClearAll();

            _mockLogMessageRepo.Setup(mlm => mlm.GetAllAsync()).ReturnsAsync(_fixture.CreateMany<LogMessage>());

            HttpResult result = new HttpResult();

            _mockGatewayService.Setup(mgs => mgs.PostLogMessageAsync(It.IsAny<DeviceLogMessage>())).Returns(Task.FromResult<HttpResult>(result));

            var loggingService = _fixture.Create<LoggingService>();

            await loggingService.UploadLoggedEventsAsync();

            _mockUserInteraction.Verify(mui => mui.Alert(It.IsAny<string>(), It.IsAny<System.Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
