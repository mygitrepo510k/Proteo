using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class CustomerCodeViewModelTests : MvxIoCSupportingTest
    {
        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            var mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(mockUserInteraction.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IReachability>(() => Mock.Of<IReachability>(r => r.IsConnected() == false));

        }

        [Fact]
        public void CustCodeVM_NoInternetDoesNotShowSpinner()
        {
            base.ClearAll();

            var ccvm =_fixture.Create<CustomerCodeViewModel>();
            ccvm.CustomerCode = "123";
            ccvm.IsBusy = false;

            ccvm.EnterCodeCommand.Execute(null);

            Assert.Equal(false, ccvm.IsBusy);
        }
    }
}
