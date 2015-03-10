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
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IReachability _reachableService;
        private readonly Portable.IReachability _reachability = null;
        private readonly Portable.ICloseApplication _closeApplication = null;
        private readonly IDataService _dataService = null; 
        private readonly IRepositories _repositories = null; 
        private readonly IDeviceInfo _deviceInfo = null; 
        private readonly IStartupService _startupService = null;
        private readonly INavigationService _navigationService = null; 
        private readonly IUserInteraction _userInteraction = null;
        private readonly ICurrentDriverRepository _currentDriver;
        private readonly IGpsService _gpsService;
        private readonly ILoggingService _loggingService = null;
        

        public StartupViewModel(IAuthenticationService authenticationService, 
                                IGatewayService gatewayService, 
                                IGatewayQueuedService gatewayQueuedService,
                                Portable.IReachability reachableService, 
                                Portable.ICloseApplication closeApplication,
                                IDataService dataService, 
                                IRepositories repositories, 
                                IDeviceInfo deviceInfo, 
                                IStartupService startupService, 
                                IUserInteraction userInteraction, 
                                IGpsService gpsService,
                                INavigationService navigationService,
                                ILoggingService loggingService)
        {
            _authenticationService = authenticationService;
            _gatewayService = gatewayService;
            _gatewayQueuedService = gatewayQueuedService;
            _reachableService = reachableService;
            _closeApplication = closeApplication;
            _dataService = dataService;
            _repositories = repositories;
            _deviceInfo = deviceInfo;
            _startupService = startupService;
            _userInteraction = userInteraction;
            _gpsService = gpsService;
            _navigationService = navigationService;
            _loggingService = loggingService;
            this.SetInitialViewModel();
        }

        private void SetInitialViewModel()
        {
            //#if DEBUG
            //  userInteraction.Confirm("DEBUGGING: clear all device setup data from the local database?", () => DEBUGGING_ClearAllData(repositories));
            //#endif

            var customerRepository = _repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
            {
                this.InitialViewModel = new PasscodeViewModel(_authenticationService, _startupService, _closeApplication, _repositories, _navigationService, _loggingService);
            }
            else
            {
                this.InitialViewModel = new CustomerCodeViewModel(_gatewayService, _reachableService, _dataService, _repositories, _userInteraction, _navigationService);
            }

#if DEBUG
            // Temporary code to stop queued items being submitted across sessions
            _repositories.GatewayQueueItemRepository.DeleteAll();
#endif
        }

        private void DEBUGGING_ClearAllData(IRepositories repositories)
        {
            repositories.ApplicationRepository.DeleteAll();
            repositories.CustomerRepository.DeleteAll();
            repositories.DeviceRepository.DeleteAll();
            repositories.DriverRepository.DeleteAll();
            repositories.GatewayQueueItemRepository.DeleteAll();
            repositories.SafetyProfileRepository.DeleteAll();
            repositories.TrailerRepository.DeleteAll();
            repositories.VehicleRepository.DeleteAll();
            repositories.VerbProfileRepository.DeleteAll();
        }
    }
}
