using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class CustomerCodeViewModelTests : MvxIoCSupportingTest
    {
        protected override void AdditionalSetup()
        {
            var mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(mockUserInteraction.Object);

            var mockOfflineReachability = new Mock<IReachability>();
            Ioc.RegisterSingleton<IReachability>(mockOfflineReachability.Object);

            mockOfflineReachability.Setup(m => m.IsConnected()).Returns(false);
        }

        [Fact]
        public void CustCodeVM_NoInternetDoesNotShowSpinner()
        {
            base.ClearAll();

            var ccvm = new CustomerCodeViewModel() { CustomerCode = "123" };

            ccvm.EnterCodeCommand.Execute(null);

            Assert.Equal(false, ccvm.IsBusy);
        }
    }
}
