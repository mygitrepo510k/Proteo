using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Portable;
using Chance.MvvmCross.Plugins.UserInteraction;
using MWF.Mobile.Core.Repositories.Interfaces;



namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {
        private readonly IAuthenticationService _authenticationService = null; 
        private readonly IGatewayService _gatewayService = null; 
        private readonly IReachability _reachableService;
        private readonly Portable.ICloseApplication _closeApplication = null;
        private readonly IDataService _dataService = null; 
        private readonly IRepositories _repositories = null; 
        private readonly IStartupService _startupService = null;
        private readonly INavigationService _navigationService = null; 
        private readonly IUserInteraction _userInteraction = null;
        private readonly ILoggingService _loggingService = null;
        
        public StartupViewModel(IAuthenticationService authenticationService, 
                                IGatewayService gatewayService, 
                                Portable.IReachability reachableService, 
                                Portable.ICloseApplication closeApplication,
                                IDataService dataService, 
                                IRepositories repositories, 
                                IStartupService startupService, 
                                IUserInteraction userInteraction, 
                                INavigationService navigationService,
                                ILoggingService loggingService)
        {
            _authenticationService = authenticationService;
            _gatewayService = gatewayService;
            _reachableService = reachableService;
            _closeApplication = closeApplication;
            _dataService = dataService;
            _repositories = repositories;
            _startupService = startupService;
            _userInteraction = userInteraction;
            _navigationService = navigationService;
            _loggingService = loggingService;

            this.SetInitialViewModel();
        }

        private void SetInitialViewModel()
        {
            var customerRepository = _repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
                this.InitialViewModel = new PasscodeViewModel(_authenticationService, _startupService, _closeApplication, _repositories, _navigationService, _loggingService);
            else
                this.InitialViewModel = new CustomerCodeViewModel(_gatewayService, _reachableService, _dataService, _repositories, _userInteraction, _navigationService);
        }

    }

}
