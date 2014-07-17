using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;


namespace MWF.Mobile.Core.Services
{

    public class AuthenticationService
        : IAuthenticationService
    {

        #region Private Members

        private IDriverRepository _driverRepository;

        #endregion

        #region Construction

        public AuthenticationService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        #endregion

        public async Task<AuthenticationResult> AuthenticateAsync(string passcode)
        {

            var test = _driverRepository.GetAll().ToList();

            Driver driver = _driverRepository.GetAll().SingleOrDefault(x => x.Passcode == passcode);

            if (driver != null)
                return new AuthenticationResult { Success = true, AuthenticationFailedMessage = null,  Driver = driver };
            else
                return new AuthenticationResult { Success = false, AuthenticationFailedMessage = "The login has failed because the passcode you have entered does not exist." };
        }

    }

}
