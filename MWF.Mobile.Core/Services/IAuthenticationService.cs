using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string AuthenticationFailedMessage { get; set; }
    }

    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string passcode);
    }

}
