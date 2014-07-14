using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class GatewayQueuedServiceTests
        :  MvxIoCSupportingTest
    {

        private List<Core.Models.GatewayQueueItem> _queueItems = null;

        protected override void AdditionalSetup()
        {
            _queueItems = new List<Core.Models.GatewayQueueItem>();
            var mockRepository = new Mock<Core.Repositories.IGatewayQueueItemRepository>();
            mockRepository.Setup(m => m.GetAllInQueueOrder()).Returns(() => _queueItems);
            mockRepository.Setup(m => m.Insert(It.IsAny<Core.Models.GatewayQueueItem>())).Callback<Core.Models.GatewayQueueItem>(gqi => _queueItems.Add(gqi));
            mockRepository.Setup(m => m.Delete(It.IsAny<Core.Models.GatewayQueueItem>())).Callback<Core.Models.GatewayQueueItem>(gqi => _queueItems.Remove(gqi));
            Ioc.RegisterSingleton<Core.Repositories.IGatewayQueueItemRepository>(mockRepository.Object);

            // Mock a success response from the BlueSphere MWF Mobile gateway service
            var responseActions = new[] { new Core.Models.GatewayServiceResponse.ResponseAction { Ack = true } };

            var response = new Core.HttpResult<Core.Models.GatewayServiceResponse.Response>
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Content = new Core.Models.GatewayServiceResponse.Response { Actions = responseActions },
            };

            var mockHttpService = new Mock<Core.Services.IHttpService>();
            mockHttpService.Setup(m => m.PostAsync<Core.Models.GatewayServiceResponse.Response>(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);
            Ioc.RegisterSingleton<Core.Services.IHttpService>(mockHttpService.Object);

            Ioc.RegisterSingleton<Core.Services.IDeviceInfoService>(Mock.Of<Core.Services.IDeviceInfoService>());
        }

        [Fact]
        public async Task GatewayQueuedService_SubmitRemovesAllItems()
        {
            base.ClearAll();

            Ioc.RegisterSingleton<Core.Portable.IReachability>(Mock.Of<Core.Portable.IReachability>(r => r.IsConnected() == true));
            var service = Ioc.IoCConstruct<Core.Services.GatewayQueuedService>();
            
            service.AddToQueue("01");
            // One item in the queue
            Assert.Equal(1, _queueItems.Count);

            await service.AddToQueueAndSubmitAsync("02");
            // No items in the queue, all have been submitted
            Assert.Equal(0, _queueItems.Count);
        }

        [Fact]
        public async Task GatewayQueuedService_NoConnectivity_QueuedItemsRetained()
        {
            base.ClearAll();

            Ioc.RegisterSingleton<Core.Portable.IReachability>(Mock.Of<Core.Portable.IReachability>(r => r.IsConnected() == false));
            var service = Ioc.IoCConstruct<Core.Services.GatewayQueuedService>();

            service.AddToQueue("01");
            // One item in the queue
            Assert.Equal(1, _queueItems.Count);

            await service.AddToQueueAndSubmitAsync("02");
            // Both items remain in the queue because there is no network connectivity
            Assert.Equal(2, _queueItems.Count);
        }

    }

}
