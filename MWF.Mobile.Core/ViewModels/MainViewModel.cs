using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class MainViewModel : BaseActivityViewModel
    {
        #region Private Members

        private List<MenuViewModel> _menuItems;
        private MvxCommand<MenuViewModel> _selectMenuItemCommand;
        private INavigationService _navigationService;

        #endregion

        public enum Option
        {
            Camera,
            SafetyCheck,
            History
        }

        #region Constructor

        public MainViewModel(IGatewayQueuedService gatewayQueuedService, IGatewayPollingService gatewayPollingService, IMobileDataRepository mobileDataRepository, INavigationService navigationService, IReachability reachability, IToast toast, IStartupService startUpService, IMainService mainService)
        {
            this.InitialViewModel = new ManifestViewModel(mobileDataRepository, navigationService, reachability, toast, gatewayPollingService, gatewayQueuedService, startUpService, mainService);

            // Start the gateway queue timer which will cause submission of any queued data to the MWF Mobile gateway service on a repeat basis
            // Commented out for now so we don't accidentally start submitting debug data to BlueSphere:
            //gatewayQueuedService.StartQueueTimer();

            //gatewayPollingService.StartPollingTimer();

            this.InitializeMenu();
        }

        #endregion   

        #region Public Properties

        public List<MenuViewModel> MenuItems
        {
            get { return this._menuItems; }
            set { this._menuItems = value; this.RaisePropertyChanged(() => this.MenuItems); }
        }

        public System.Windows.Input.ICommand SelectMenuItemCommand
        {
            get
            {
                return this._selectMenuItemCommand ?? (this._selectMenuItemCommand = new MvxCommand<MenuViewModel>(this.DoSelectMenuItemCommand));
            }
        }

        #endregion 
        
        #region Private Methods

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

        private void DoSelectMenuItemCommand(MenuViewModel item)
        {
            
            switch (item.Option)
            {
                case Option.Camera:
                    this.ShowViewModel<CameraViewModel>();
                    break;
                case Option.SafetyCheck:
                    break;
                case Option.History:
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

}
