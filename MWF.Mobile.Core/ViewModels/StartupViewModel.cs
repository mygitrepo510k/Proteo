using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

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
        private readonly IInfoService _infoService = null;
        private readonly INavigationService _navigationService = null; 
        private readonly ICustomUserInteraction _userInteraction = null;
        private readonly ILoggingService _loggingService = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        
        public StartupViewModel(IAuthenticationService authenticationService, 
                                IGatewayService gatewayService, 
                                Portable.IReachability reachableService, 
                                Portable.ICloseApplication closeApplication,
                                IDataService dataService, 
                                IRepositories repositories, 
                                IInfoService infoService, 
                                ICustomUserInteraction userInteraction, 
                                INavigationService navigationService,
                                ILoggingService loggingService,
                                IGatewayQueuedService gatewayQueuedService)
        {
            _authenticationService = authenticationService;
            _gatewayService = gatewayService;
            _reachableService = reachableService;
            _closeApplication = closeApplication;
            _dataService = dataService;
            _repositories = repositories;
            _infoService = infoService;
            _userInteraction = userInteraction;
            _navigationService = navigationService;
            _loggingService = loggingService;
            _gatewayQueuedService = gatewayQueuedService;

            this.SetInitialViewModel();
        }

        private void SetInitialViewModel()
        {


            var customerRepository = _repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
                this.InitialViewModel = new PasscodeViewModel(_authenticationService, _infoService, _closeApplication, _repositories, _navigationService, _loggingService, _gatewayQueuedService);
            else
                this.InitialViewModel = new CustomerCodeViewModel(_gatewayService, _reachableService, _dataService, _repositories, _userInteraction, _navigationService);
        }

    }

}
