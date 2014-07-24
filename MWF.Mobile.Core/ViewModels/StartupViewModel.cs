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

        public StartupViewModel(IAuthenticationService authenticationService, 
                                IGatewayService gatewayService, 
                                IGatewayQueuedService gatewayQueuedService, 
                                Portable.IReachability reachableService, 
                                IDataService dataService, 
                                IRepositories repositories, 
                                IDeviceInfo deviceInfo, 
                                IStartupService startupService, 
                                IUserInteraction userInteraction, 
                                ICurrentDriverRepository currentDriver,
                                IGpsService gpsService)

        {
//#if DEBUG
//            userInteraction.Confirm("DEBUGGING: clear all device setup data from the local database?", () => DEBUGGING_ClearAllData(repositories));
//#endif
            //this.InitialViewModel = new ManifestViewModel();
            

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

            var customerRepository = repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
            {
                this.InitialViewModel = new PasscodeViewModel(authenticationService, startupService,currentDriver);
            }
            else
            {
                this.InitialViewModel = new CustomerCodeViewModel(gatewayService, reachableService, dataService, repositories, userInteraction);
            }
             
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
