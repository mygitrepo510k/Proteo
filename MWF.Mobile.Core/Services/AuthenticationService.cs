﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;


namespace MWF.Mobile.Core.Services
{

    public class AuthenticationService
        : IAuthenticationService
    {

        #region Private Members

        private IDriverRepository _driverRepository;
        private IReachability _reachability;
        private IGatewayService _gatewayService;

        #endregion

        #region Construction

        public AuthenticationService(IDriverRepository driverRepository, IReachability reachability, IGatewayService gatewayService)
        {
            _driverRepository = driverRepository;
            _reachability = reachability;
            _gatewayService = gatewayService;
        }

        #endregion

        public async Task<AuthenticationResult> AuthenticateAsync(string passcode)
        {

            Driver driver = GetMatchingDriver(passcode);

            // driver not in local DB, update from BlueSphere (if we can)
            if (driver == null && _reachability.IsConnected())
            {
                await UpdateDrivers();
                driver = GetMatchingDriver(passcode);
            }

            if (driver != null)
                return new AuthenticationResult { Success = true, AuthenticationFailedMessage = null,  Driver = driver };
            else
                return new AuthenticationResult { Success = false, AuthenticationFailedMessage = "The driver passcode you submitted doesn't exist, check the passcode and try again." };
        }

        #region Private Methods

        private Driver GetMatchingDriver(string passcode)
        {
            return _driverRepository.GetAll().SingleOrDefault(x => x.Passcode == passcode);
        }

        private async Task UpdateDrivers()
        {
            IEnumerable<Driver> drivers = await _gatewayService.GetDrivers();
            _driverRepository.DeleteAll();
            _driverRepository.Insert(drivers);           
        }

        #endregion

    }

}
