using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
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
using MWF.Mobile.Core.Repositories.Interfaces;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Extensions;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class MessageViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;
        private Mock<IMainService> _mockMainService;



        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";
            _mobileData.Order.Type = InstructionType.OrderMessage;

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);

        }

        #endregion Setup

        #region Test

        /// <summary>
        /// This is to test setup for the view model for if its a message with a point.
        /// This is determined if the mobile data has a point (MobileData.Order.Addresses)
        /// </summary>
        [Fact]
        public void MessageVM_MessageWithPoint_Setup()
        {
            base.ClearAll();

            var MessageVM = _fixture.Create<MessageViewModel>();

            MessageVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Message with a Point", MessageVM.FragmentTitle);
            Assert.Equal(true, MessageVM.isWithPoint);
            Assert.Equal(_mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode, MessageVM.Address);

        }

        /// <summary>
        /// This is to test setup for the view model for if its a message.
        /// This is determined if the mobile data is null (MobileData.Order.Addresses)
        /// </summary>
        [Fact]
        public void MessageVM_Message_Setup()
        {
            base.ClearAll();

            _mobileData.Order.Addresses = new List<Address>();

            var MessageVM = _fixture.Create<MessageViewModel>();

            MessageVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Message", MessageVM.FragmentTitle);
            Assert.Equal(false, MessageVM.isWithPoint);
            Assert.Equal(string.Empty, MessageVM.Address);

        }

        [Fact]
        public void MessageVM_Message_ReadButton()
        {
            base.ClearAll();

            _mobileData.Order.Addresses = new List<Address>();

            var MessageVM = _fixture.Create<MessageViewModel>();

            MessageVM.Init(new NavData<MobileData>() { Data = _mobileData });

            MessageVM.ReadMessageCommand.Execute(null);

            _mockNavigationService.Verify(mns => mns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Once);

        }

        [Fact]
        public void MessageVM_MessageWithPoint_ReadButton()
        {
            base.ClearAll();

            var MessageVM = _fixture.Create<MessageViewModel>();

            MessageVM.Init(new NavData<MobileData>() { Data = _mobileData });

            MessageVM.ReadMessageCommand.Execute(null);

            _mockNavigationService.Verify(mns => mns.MoveToNext(It.IsAny<NavData<MobileData>>()), Times.Once);

        }


        #endregion Test

    }
}
