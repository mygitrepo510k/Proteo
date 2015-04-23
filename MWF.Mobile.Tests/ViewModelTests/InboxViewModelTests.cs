using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
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
        private Mock<IMainService> _mockMainService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IGatewayPollingService> _mockGatewayPollingService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _driver = _fixture.Create<Driver>();

            _mockMainService = _fixture.InjectNewMock<IMainService>();
            _mockMainService.Setup(ms => ms.CurrentDriver).Returns(_driver);

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

            for(var i = 0; i < 4; i++)
            {
                var message = _fixture.Create<MobileData>();
                message.Order.Type = Core.Enums.InstructionType.OrderMessage;
                messages.Add(message);
            }

            _mobileDataRepoMock.Setup(ms => ms.GetAllMessages(It.Is<Guid>(i => i == _driver.ID))).Returns(messages);

            var inboxVM = _fixture.Create<InboxViewModel>();

            inboxVM.RefreshMessagesCommand.Execute(null);

            //Its twice because when the viewmodel is created then it calls refreshMessages()
            _mobileDataRepoMock.Verify(md => md.GetAllMessages(It.Is<Guid>(i => i == _driver.ID)), Times.Exactly(2));
            _mockGatewayPollingService.Verify(gp => gp.PollForInstructions(), Times.Exactly(2));

            Assert.Equal(4, inboxVM.MessagesCount);
            Assert.Equal("Showing 4 messages", inboxVM.InboxHeaderText);
        
        }

        #endregion Tests
    }
}
