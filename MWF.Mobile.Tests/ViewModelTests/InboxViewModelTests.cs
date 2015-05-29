using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InboxViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private Driver _driver;
        private Mock<IMobileDataRepository> _mobileDataRepoMock;
        private Mock<IInfoService> _mockInfoService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IGatewayPollingService> _mockGatewayPollingService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _driver = _fixture.Create<Driver>();

            _mockInfoService = _fixture.InjectNewMock<IInfoService>();
            _mockInfoService.Setup(ms => ms.LoggedInDriver).Returns(_driver);

            _mobileDataRepoMock = _fixture.InjectNewMock<IMobileDataRepository>();

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();
            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockGatewayPollingService = _fixture.InjectNewMock<IGatewayPollingService>();

            Ioc.RegisterSingleton<IMvxMessenger>(_fixture.InjectNewMock<IMvxMessenger>().Object);

        } 

        #endregion Setup

        #region Tests

        [Fact]
        public void InstructionVM_FragmentTitle()
        {
            base.ClearAll();

            var inboxVM = _fixture.Create<InboxViewModel>();

            Assert.Equal("Inbox", inboxVM.FragmentTitle);

        }

        [Fact]
        public void InstructionVM_RefreshMessages()
        {
            base.ClearAll();

            List<MobileData> messages = new List<MobileData>();
            int validMessageCount = 0;

            for(var i = 0; i < 4; i++)
            {
                var message = _fixture.Create<MobileData>();
                message.Order.Type = Core.Enums.InstructionType.OrderMessage;
                messages.Add(message);

                if (message.EffectiveDate > DateTime.Today.AddDays(-7))
                    validMessageCount++;
            }

            _mobileDataRepoMock.Setup(ms => ms.GetAllMessages(It.Is<Guid>(i => i == _driver.ID))).Returns(messages);

            var inboxVM = _fixture.Create<InboxViewModel>();

            inboxVM.RefreshMessagesCommand.Execute(null);

            //Its twice because when the viewmodel is created then it calls refreshMessages()
            _mobileDataRepoMock.Verify(md => md.GetAllMessages(It.Is<Guid>(i => i == _driver.ID)), Times.Exactly(2));
            _mockGatewayPollingService.Verify(gp => gp.PollForInstructionsAsync(), Times.Exactly(2));

            Assert.Equal(validMessageCount, inboxVM.MessagesCount);
            Assert.Equal("Showing " + validMessageCount + " messages", inboxVM.InboxHeaderText);
        
        }

        [Fact]
        public void InboxVM_CheckInstructionNotification()
        {

            base.ClearAll();

            var inboxVM = _fixture.Create<InboxViewModel>();

            inboxVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Add, new Guid());

            //Its twice because when the viewmodel is created then it calls refreshMessages()
            _mobileDataRepoMock.Verify(md => md.GetAllMessages(It.Is<Guid>(i => i == _driver.ID)), Times.Exactly(2));

            //Should only pull from the database because new instructions would of just been inserted
            //Gets called once when the inbox is created.
            _mockGatewayPollingService.Verify(gp => gp.PollForInstructionsAsync(), Times.Once);

        }

        #endregion Tests
    }
}
