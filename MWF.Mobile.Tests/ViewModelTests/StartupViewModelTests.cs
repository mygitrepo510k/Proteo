﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.ViewModels;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class StartupViewModelTests : MvxIoCSupportingTest
    {
        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

        }

        [Fact]
        public void StartupVM_EmptyCustomerRepository()
        {
            base.ClearAll();

            // Customer repository will return an empty list
            _fixture.Inject<ICustomerRepository>(Mock.Of<ICustomerRepository>(cr => cr.GetAllAsync() == Task.FromResult(Enumerable.Empty<Customer>())));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var startUpViewModel = _fixture.Create<StartupViewModel>();

            // startup view model should return a customer code view model
            Assert.IsType(typeof(CustomerCodeViewModel), startUpViewModel.InitialViewModel);

        }

        [Fact]
        public void StartupVM_NonEmptyCustomerRepository()
        {
            base.ClearAll();

            _fixture.Inject<ICustomerRepository>(Mock.Of<ICustomerRepository>(cr => cr.GetAllAsync() == Task.FromResult(_fixture.CreateMany<Customer>())));
            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            var startUpViewModel = _fixture.Create<StartupViewModel>();

            // startup view model should return a passcode view model
            Assert.IsType(typeof(PasscodeViewModel), startUpViewModel.InitialViewModel);

        }
    }
}
