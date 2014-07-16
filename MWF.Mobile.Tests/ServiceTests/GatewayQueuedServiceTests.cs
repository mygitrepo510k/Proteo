using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models;
using Xunit;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class GatewayQueuedServiceTests
        :  MvxIoCSupportingTest
    {

        private List<Core.Models.GatewayQueueItem> _queueItems = null;

        private IFixture _fixture;

        private Mock<IGatewayQueueItemRepository> _mockQueueItemRepository;

        protected override void AdditionalSetup()
        {
            _queueItems = new List<Core.Models.GatewayQueueItem>();
            _mockQueueItemRepository = new Mock<Core.Repositories.IGatewayQueueItemRepository>();
            _mockQueueItemRepository.Setup(m => m.GetAllInQueueOrder()).Returns(() => _queueItems);
            _mockQueueItemRepository.Setup(m => m.Insert(It.IsAny<Core.Models.GatewayQueueItem>())).Callback<Core.Models.GatewayQueueItem>(gqi => _queueItems.Add(gqi));
            _mockQueueItemRepository.Setup(m => m.Delete(It.IsAny<Core.Models.GatewayQueueItem>())).Callback<Core.Models.GatewayQueueItem>(gqi => _queueItems.Remove(gqi));
           

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            
            IDeviceRepository repo = Mock.Of<IDeviceRepository>(dr => dr.GetAll() == _fixture.CreateMany<Device>());
            IRepositories repos = Mock.Of<IRepositories>(r => r.DeviceRepository == repo &&
                                                              r.GatewayQueueItemRepository == _mockQueueItemRepository.Object);
            _fixture.Register<IRepositories>(() => repos);

            // Mock a success response from the BlueSphere MWF Mobile gateway service
            var responseActions = new[] { new Core.Models.GatewayServiceResponse.ResponseAction { Ack = true } };

            var response = new Core.HttpResult<Core.Models.GatewayServiceResponse.Response>
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Content = new Core.Models.GatewayServiceResponse.Response { Actions = responseActions },
            };

            var mockHttpService = new Mock<Core.Services.IHttpService>();
            mockHttpService.Setup(m => m.PostAsync<Core.Models.GatewayServiceResponse.Response>(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);
            _fixture.Register<Core.Services.IHttpService>(() => mockHttpService.Object);

            _fixture.Register<Core.Services.IDeviceInfo>(() => Mock.Of<Core.Services.IDeviceInfo>());

            var messenger = new MvxMessengerHub();
            _fixture.Register<IMvxMessenger>(() => messenger);

            // We don't have the GatewayQueueTimerService so replicate the trigger -> publish elapsed message functionality
            var token = messenger.Subscribe<Core.Messages.GatewayQueueTimerCommandMessage>(m =>
            {
                if (m.Command == Core.Messages.GatewayQueueTimerCommandMessage.TimerCommand.Trigger)
                    messenger.Publish(new Core.Messages.GatewayQueueTimerElapsedMessage(this));
            });
        }

        [Fact]
        public void GatewayQueuedService_SubmitRemovesAllItems()
        {
            base.ClearAll();

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected() == true));
            var service =_fixture.Create<Core.Services.GatewayQueuedService>();
            service.StartQueueTimer();

            service.AddToQueue("01");
            // One item in the queue
            Assert.Equal(1, _queueItems.Count);

            service.AddToQueueAndSubmit("02");
            // No items in the queue, all have been submitted
            Assert.Equal(0, _queueItems.Count);
        }

        [Fact]
        public void GatewayQueuedService_NoConnectivity_QueuedItemsRetained()
        {
            base.ClearAll();

            _fixture.Register<Core.Portable.IReachability>(() => Mock.Of<Core.Portable.IReachability>(r => r.IsConnected() == false));
            var service =_fixture.Create<Core.Services.GatewayQueuedService>();
            service.StartQueueTimer();

            service.AddToQueue("01");
            // One item in the queue
            Assert.Equal(1, _queueItems.Count);

            service.AddToQueueAndSubmit("02");
            // Both items remain in the queue because there is no network connectivity
            Assert.Equal(2, _queueItems.Count);
        }

    }

}
