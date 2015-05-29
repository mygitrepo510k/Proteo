using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionTrailerViewModelTests
        :  MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private Core.Models.Trailer _trailer;
        private TrailerItemViewModel _trailerItemViewModel;
        private IStartupService _startupService;
        private Mock<IMvxMessenger> _mockMessenger;
        private Mock<INavigationService> _navigationServiceMock;
        private Mock<ICustomUserInteraction> _mockUserInteraction;


        protected override void AdditionalSetup()
        {
            var mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(mockDispatcher);

            var mockUserInteraction = new Mock<ICustomUserInteraction>();
            Ioc.RegisterSingleton<ICustomUserInteraction>(mockUserInteraction.Object);

            _mockUserInteraction = new Mock<ICustomUserInteraction>();
            _mockUserInteraction.Setup(ui => ui.Confirm(It.IsAny<string>(), It.IsAny<Action<bool>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));
            Ioc.RegisterSingleton<ICustomUserInteraction>(_mockUserInteraction.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customize<TrailerListViewModel>(tlvm => tlvm.Without(x => x.DefaultTrailerReg));

            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == true));

            _trailer = new Core.Models.Trailer() { Registration = "TestRegistration", ID = Guid.NewGuid() };
            _trailerItemViewModel = new TrailerItemViewModel() { Trailer = _trailer };

            var infoService = _fixture.Create<InfoService>();
            _fixture.Inject<IInfoService>(infoService);
            _startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(_startupService);

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            _navigationServiceMock = _fixture.InjectNewMock<INavigationService>();
            Ioc.RegisterSingleton<INavigationService>(_navigationServiceMock.Object);
        }




        /// <summary>
        /// Tests the view model can be initialized correctly
        /// </summary>
        [Fact]
        public void InstructionTrailerListVM_Initialization()
        {
            base.ClearAll();

            var trailerRepository = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Core.Models.Trailer>();
            trailerRepository.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<InstructionTrailerViewModel>();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false, true, null);
            var navData = new NavData<MobileData>() { Data = mobileData };
            //set the trailer in the current order to have the same registration as first trailer
            mobileData.Order.Additional.Trailer.TrailerId = trailers.First().Registration;

            vm.Init(navData);

            Assert.Equal(vm.DefaultTrailerReg, mobileData.Order.Additional.Trailer.TrailerId);

            // first item in list should be the "default" i.e. it is the one specified in the order
            Assert.True(vm.Trailers.First().IsDefault);
            Assert.True(vm.Trailers.First().TrailerText.EndsWith("(as order)"));

        }

        /// <summary>
        /// Tests that when a trailer is selected, the updated trailer is stored in the nav data
        /// and the next view model is navigated to
        /// </summary>
        [Fact]
        public void InstructionTrailerListVM_TrailerSelection()
        {
            base.ClearAll();

            var trailerRepository = new Mock<ITrailerRepository>();
            var trailers = _fixture.CreateMany<Core.Models.Trailer>();
            trailerRepository.Setup(vr => vr.GetAll()).Returns(trailers);

            _fixture.Inject<ITrailerRepository>(trailerRepository.Object);
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var vm = _fixture.Create<InstructionTrailerViewModel>();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false, true, null);
            var navData = new NavData<MobileData>() { Data = mobileData };

            vm.Init(navData);

            var trailerItem = vm.Trailers.First();

            var mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();
            mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Confirm your trailer");

            //select the first trailer
            vm.TrailerSelectCommand.Execute(trailerItem);

            //Should have set the updated trailer on the nav data
            Assert.Equal(navData.OtherData["UpdatedTrailer"], trailerItem.Trailer);

            _navigationServiceMock.Verify( ns => ns.MoveToNext(It.Is<NavData<MobileData>>(nd => nd == navData)));


        }

        [Fact]
        public async Task InstructionTrailerListVM_CheckInstructionNotification_Delete()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false, true, null);
            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };

            var vm = _fixture.Create<InstructionTrailerViewModel>();

            vm.Init(new NavData<MobileData>() { Data = mobileData });

            await vm.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationServiceMock.Verify(ns => ns.GoToManifest(), Times.Once);

        }


        [Fact]
        public async Task InstructionTrailerListVM_CheckInstructionNotification_Update_Confirm()
        {
            base.ClearAll();

            var mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false, true, null);

            var mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == mobileData.ID))).Returns(mobileData);
            var repositories = _fixture.Create<Repositories>();
            _fixture.Inject<IRepositories>(repositories);

            // set the trailer the user has selected to be the same as current trailer and the one specified on the order
            var navData = new NavData<MobileData>() { Data = mobileData };

            var vm = _fixture.Create<InstructionTrailerViewModel>();

            vm.Init(new NavData<MobileData>() { Data = mobileData });

            await vm.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            mockMobileDataRepo.Verify(mdr => mdr.GetByID(It.Is<Guid>(gui => gui.ToString() == mobileData.ID.ToString())), Times.Exactly(1));

        }

      

        
    }
}
