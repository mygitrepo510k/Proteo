﻿using Cirrious.MvvmCross.Community.Plugins.Sqlite;
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

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class InstructionViewModelTests
        : MvxIoCSupportingTest
    {
        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mobileDataRepo;
        private Mock<INavigationService> _navigationService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.GroupTitle = "Run1010";

            _mobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();
;
        }



        [Fact]
        public void InstructionVM_FragmentTitle()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Type.ToString(), instructionVM.FragmentTitle);

        }

        [Fact]
        public void InstructionVM_RunID()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.GroupTitleFormatted, instructionVM.RunID);

        }

        [Fact]
        public void InstructionVM_ArriveDateTime()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Arrive.ToString(), instructionVM.ArriveDateTime);

        }

        [Fact]
        public void InstructionVM_ArriveDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Arrive = new DateTime();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(string.Empty, instructionVM.ArriveDateTime);

        }

        [Fact]
        public void InstructionVM_DepartDateTime()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Depart.ToString(), instructionVM.DepartDateTime);

        }

        [Fact]
        public void InstructionVM_DepartDateTime_BlankIfDefaultDate()
        {
            base.ClearAll();

            _mobileData.Order.Depart = new DateTime();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(string.Empty, instructionVM.DepartDateTime);

        }

        [Fact]
        public void InstructionVM_Address()
        {
            base.ClearAll();

            _mobileData.Order.Addresses[0].Lines = "Testline1|TestLine2|TestLine3";

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Addresses[0].Lines.Replace("|","\n") + "\n" + _mobileData.Order.Addresses[0].Postcode, instructionVM.Address);

        }

        [Fact]
        public void InstructionVM_ProgressButtonText_NotStarted()
        {
            base.ClearAll();         

            var instructionVM = _fixture.Create<InstructionViewModel>();

            _mobileData.ProgressState = Core.Enums.InstructionProgress.NotStarted;

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("Drive", instructionVM.ProgressButtonText);

            instructionVM.ProgressInstructionCommand.Execute(null);

            Assert.True(_mobileData.ProgressState == Core.Enums.InstructionProgress.Driving);

            _mobileDataRepo.Verify(mdr => mdr.Update(It.Is<MobileData>(md => md == _mobileData)), Times.Once);




        }

        [Fact]
        public void InstructionVM_ProgressButton_Driving()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            _mobileData.ProgressState = Core.Enums.InstructionProgress.Driving;

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("On Site", instructionVM.ProgressButtonText);

            instructionVM.ProgressInstructionCommand.Execute(null);

            Assert.True(_mobileData.ProgressState == Core.Enums.InstructionProgress.OnSite);

            _mobileDataRepo.Verify(mdr => mdr.Update(It.Is<MobileData>(md => md == _mobileData)), Times.Once);

        }

        [Fact]
        public void InstructionVM_ProgressButton_OnSite()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            _mobileData.ProgressState = Core.Enums.InstructionProgress.OnSite;

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.Equal("On Site", instructionVM.ProgressButtonText);

            instructionVM.ProgressInstructionCommand.Execute(null);

            // Shouldn't have set to complete yet
            Assert.True(_mobileData.ProgressState == Core.Enums.InstructionProgress.OnSite);

            // Should have told navigation service to move on
            _navigationService.Verify(ns => ns.MoveToNext(It.Is<NavItem<MobileData>>(ni => ni.ID == _mobileData.ID )), Times.Once);

        }

        [Fact]
        public void InstructionVM_ProgressButton_SelectOrder()
        {
            base.ClearAll();

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Item order = _mobileData.Order.Items[0];

            instructionVM.ShowOrderCommand.Execute(order);

            // Should have told navigation service to move on
            _navigationService.Verify(ns => ns.MoveToNext(It.Is<NavItem<Item>>(ni => ni.ID == order.ID && ni.ParentID == _mobileData.ID)), Times.Once);


        }


        [Fact]
        public void InstructionVM_ChangeTrailerAllowed_DeliveryInstruction()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Deliver;

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.False(instructionVM.ChangeTrailerAllowed);

        }

        [Fact]
        public void InstructionVM_ChangeTrailerAllowed_CollectInstruction_TrailerConfirmationEnabled()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.IsTrailerConfirmationEnabled = true;

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.True(instructionVM.ChangeTrailerAllowed);

        }

        [Fact]
        public void InstructionVM_ChangeTrailerAllowed_CollectInstruction_TrailerConfirmationDisabled()
        {
            base.ClearAll();

            _mobileData.Order.Type = Core.Enums.InstructionType.Collect;
            _mobileData.Order.Additional.IsTrailerConfirmationEnabled = false;

            var instructionVM = _fixture.Create<InstructionViewModel>();

            instructionVM.Init(new NavItem<MobileData>() { ID = _mobileData.ID });

            Assert.False(instructionVM.ChangeTrailerAllowed);

        }

    }
}