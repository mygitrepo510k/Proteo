using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public class AuthenticationService
        : IAuthenticationService
    {

        public AuthenticationService()
        { }

        public async Task<AuthenticationResult> AuthenticateAsync(string passcode)
        {
            //TODO: implement authentication against driver list in local database
            if (passcode == "9999")
                return new AuthenticationResult { Success = true, AuthenticationFailedMessage = null };
            else
                return new AuthenticationResult { Success = false, AuthenticationFailedMessage = "Login failed: the passcode is 9999" };
        }

    }

}
