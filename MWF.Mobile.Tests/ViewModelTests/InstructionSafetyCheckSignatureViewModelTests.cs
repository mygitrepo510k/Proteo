﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using Xunit;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Tests.Helpers;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Tests.ViewModelTests
{
    
    public class InstructionSafetyCheckFaultViewModelTests
        : MvxIoCSupportingTest
    {

        private IFixture _fixture;
        private IStartupService _startupService;
        private Mock<IUserInteraction> _mockUserInteraction;
        private Mock<IMvxMessenger> _mockMessenger;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private Core.Models.Trailer _trailer;
        private Mock<ISafetyProfileRepository> _mockSafetyProfileRepository;
        private Mock<INavigationService> _mockNavigationService;
        private Core.Models.Driver _driver;
        IEnumerable<SafetyProfile> _safetyProfiles;
        private SafetyProfile _trailerSafetyProfile;
        private SafetyProfile _vehicleSafetyProfile;
        private SafetyCheckData _vehicleSafetyCheckData;
        private SafetyCheckData _trailerSafetyCheckData;

        protected override void AdditionalSetup()
        {

            _mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(_mockUserInteraction.Object);

            _mockMessenger = new Mock<IMvxMessenger>();
            Ioc.RegisterSingleton<IMvxMessenger>(_mockMessenger.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyCheckItemViewModels));
            _fixture.Customize<InstructionSafetyCheckViewModel>(vm => vm.Without(x => x.SafetyProfileVehicle));
            _fixture.Customize<SafetyCheckFault>( vm =>vm.With( x => x.Status, Core.Enums.SafetyCheckStatus.Passed));

            _driver = _fixture.Create<Driver>();

            _startupService = _fixture.Create<StartupService>();
            _fixture.Inject<IStartupService>(_startupService);
            _startupService.LoggedInDriver = _driver;


            _mobileData = _fixture.SetUpInstruction(Core.Enums.InstructionType.Collect, false, true, false, false, false, false,true, null);
            _navData = new NavData<MobileData>() { Data = _mobileData };

            _startupService.CurrentVehicleSafetyCheckData= _vehicleSafetyCheckData = _fixture.Create<SafetyCheckData>();
            _navData.OtherData["UpdatedTrailerSafetyCheckData"] =_trailerSafetyCheckData = _fixture.Create<SafetyCheckData>();

            _trailer = _fixture.Create<Core.Models.Trailer>();
            _navData.OtherData["UpdatedTrailer"] = _trailer;

            var mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);


            _mockSafetyProfileRepository = _fixture.InjectNewMock<ISafetyProfileRepository>();

            var repositories = _fixture.Create<Repositories>();
            _fixture.Inject<IRepositories>(repositories);

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();
            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);

            _safetyProfiles = CreateSafetyProfiles();
            

        }

        private IEnumerable<SafetyProfile> CreateSafetyProfiles()
        {
            _trailerSafetyProfile = _fixture.Create<SafetyProfile>();
            _trailerSafetyProfile.IsTrailerProfile = true;
            _trailerSafetyProfile.IntLink = _trailer.SafetyCheckProfileIntLink;

            _vehicleSafetyProfile = _fixture.Create<SafetyProfile>();
            _vehicleSafetyProfile.IsTrailerProfile = false;
            _vehicleSafetyProfile.IntLink = _startupService.CurrentVehicle.SafetyCheckProfileIntLink;

            List<SafetyProfile> profiles = new List<SafetyProfile>() { _trailerSafetyProfile, _vehicleSafetyProfile };

            return profiles;
        }



        // Checks that after initialization and selecting "done" the safety check is signed with the signature and the navigation service is called
        [Fact]
        public void InstructionSafetyCheckSignatureVM_InitandDone()
        {
            base.ClearAll();

            _mockSafetyProfileRepository.Setup(spr => spr.GetAll()).Returns(_safetyProfiles);

            var vm = _fixture.Create<InstructionSafetyCheckSignatureViewModel>();

            vm.Init(_navData);

            vm.DoneCommand.Execute(null);

            //check the sfaety data has been signed
            Assert.Equal(vm.SignatureEncodedImage, _startupService.CurrentVehicleSafetyCheckData.Signature.EncodedImage);
            Assert.Equal(vm.SignatureEncodedImage, _trailerSafetyCheckData.Signature.EncodedImage);

            //check the next view model was navigated to
            _mockNavigationService.Verify(ns => ns.MoveToNext(It.Is<NavData<MobileData>>(x => x == _navData)), Times.Once);

        }



    }

}