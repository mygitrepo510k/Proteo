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

        public enum Option
        {
            Camera,
            SafetyCheck,
            History
        }

        private readonly IAuthenticationService _authenticationService = null; 
        private readonly IGatewayService _gatewayService = null; 
        private readonly IGatewayQueuedService _gatewayQueuedService = null; 
        private readonly Portable.IReachability _reachability = null; 
        private readonly IDataService _dataService = null; 
        private readonly IRepositories _repositories = null; 
        private readonly IDeviceInfo _deviceInfo = null; 
        private readonly IStartupService _startupService = null; 
        private readonly IUserInteraction _userInteraction = null; 
        private readonly IGpsService _gpsService = null;

        public StartupViewModel(IAuthenticationService authenticationService, 
                                IGatewayService gatewayService, 
                                IGatewayQueuedService gatewayQueuedService, 
                                Portable.IReachability reachability, 
                                IDataService dataService, 
                                IRepositories repositories, 
                                IDeviceInfo deviceInfo, 
                                IStartupService startupService, 
                                IUserInteraction userInteraction, 
                                IGpsService gpsService)
        {
            _authenticationService = authenticationService;
            _gatewayService = gatewayService;
            _gatewayQueuedService = gatewayQueuedService;
            _reachability = reachability;
            _dataService = dataService;
            _repositories = repositories;
            _deviceInfo = deviceInfo;
            _startupService = startupService;
            _userInteraction = userInteraction;
            _gpsService = gpsService;

#if DEBUG
            // Uncomment the following line to clear all data on startup
            //DEBUGGING_ClearAllData(repositories);
#endif

            this.SetInitialViewModel();
            this.InitializeMenu();
        }

        private void SetInitialViewModel()
        {
            var customerRepository = _repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
                this.InitialViewModel = new PasscodeViewModel(_authenticationService, _startupService, _repositories.CurrentDriverRepository);
            else
                this.InitialViewModel = new CustomerCodeViewModel(_gatewayService, _reachability, _dataService, _repositories, _userInteraction);
        }

        private void InitializeMenu()
        {
            _menuItems = new List<MenuViewModel>
            {
                new MenuViewModel
                {
                    Option = Option.Camera,
                    Text = "Camera"
                },
                new MenuViewModel
                {
                    Option = Option.History,
                    Text = "History"
                },
                new MenuViewModel
                {
                    Option = Option.SafetyCheck,
                    Text = "SafetyCheck"
                },
            };
        }

        private void DEBUGGING_ClearAllData(IRepositories repositories)
        {
            repositories.ApplicationRepository.DeleteAll();
            repositories.ConfigRepository.DeleteAll();
            repositories.CustomerRepository.DeleteAll();
            repositories.DeviceRepository.DeleteAll();
            repositories.DriverRepository.DeleteAll();
            repositories.GatewayQueueItemRepository.DeleteAll();
            repositories.SafetyProfileRepository.DeleteAll();
            repositories.TrailerRepository.DeleteAll();
            repositories.VehicleRepository.DeleteAll();
            repositories.VerbProfileRepository.DeleteAll();
        }

        private List<MenuViewModel> _menuItems;
        public List<MenuViewModel> MenuItems
        {
            get { return this._menuItems; }
            set { this._menuItems = value; this.RaisePropertyChanged(() => this.MenuItems); }
        }

        private MvxCommand<MenuViewModel> _selectMenuItemCommand;
        public System.Windows.Input.ICommand SelectMenuItemCommand
        {
            get
            {
                return this._selectMenuItemCommand ?? (this._selectMenuItemCommand = new MvxCommand<MenuViewModel>(this.DoSelectMenuItemCommand));
            }
        }

        private void DoSelectMenuItemCommand(MenuViewModel item)
        {
            
        }

    }

}
