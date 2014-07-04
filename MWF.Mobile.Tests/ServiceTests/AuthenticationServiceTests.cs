using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class AuthenticationServiceTests
        : MvxIoCSupportingTest
    {

        /// <summary>
        /// Temporary test to ensure framework is set up correctly
        /// </summary>
        [Fact]
        public async Task AuthenticationService_9999AuthenticatesSuccessfully()
        {
            base.ClearAll();

            var service = new AuthenticationService();
            var result = await service.AuthenticateAsync("9999");

            Assert.True(result.Success);
        }

    }

}
