using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using Cirrious.CrossCore;
using Chance.MvvmCross.Plugins.UserInteraction;
using MWF.Mobile.Core.Repositories;

namespace MWF.Mobile.Core.ViewModels
{

    public class MainViewModel : BaseActivityViewModel
    {
        #region Private Members

        private List<MenuViewModel> _menuItems;
        private MvxCommand<MenuViewModel> _selectMenuItemCommand;
        private MvxCommand _logoutCommand;
        private INavigationService _navigationService;

        #endregion

        public enum Option
        {
            Camera,
            ViewSafetyCheck,
            RunNewSafetyCheck,
            Manifest
        }

        #region Constructor

        public MainViewModel(IGatewayQueuedService gatewayQueuedService, IGatewayPollingService gatewayPollingService, IRepositories repositories, INavigationService navigationService, IReachability reachability, IToast toast, IStartupService startUpService, IMainService mainService)
        {
            this.InitialViewModel = new ManifestViewModel(repositories, navigationService, reachability, toast, gatewayPollingService, gatewayQueuedService, startUpService, mainService);

            // Start the gateway queue timer which will cause submission of any queued data to the MWF Mobile gateway service on a repeat basis
            // Commented out for now so we don't accidentally start submitting debug data to BlueSphere:
            //gatewayQueuedService.StartQueueTimer();

            //gatewayPollingService.StartPollingTimer();
            _navigationService = navigationService;

            this.InitializeMenu();
        }

        #endregion

        #region Public Properties

        public List<MenuViewModel> MenuItems
        {
            get { return this._menuItems; }
            set { this._menuItems = value; this.RaisePropertyChanged(() => this.MenuItems); }
        }

        public string LogoutText
        {
            get { return "Logout"; }
        }

        public System.Windows.Input.ICommand SelectMenuItemCommand
        {
            get
            {
                return this._selectMenuItemCommand ?? (this._selectMenuItemCommand = new MvxCommand<MenuViewModel>(this.DoSelectMenuItemCommand));
            }
        }

        public System.Windows.Input.ICommand LogoutCommand
        {
            get
            {
                return this._logoutCommand ?? (this._logoutCommand = new MvxCommand(this.DoLogoutCommand));
            }
        }

        #endregion

        #region Private Methods

        private void InitializeMenu()
        {
            MenuItems = new List<MenuViewModel>
            {
                new MenuViewModel
                {
                    Option = Option.Manifest,
                    Text = "Manifest Screen"
                },
                new MenuViewModel
                {
                    Option = Option.Camera,
                    Text = "Camera"
                },
                new MenuViewModel
                {
                    Option = Option.ViewSafetyCheck,
                    Text = "View Safety Check"
                },
                new MenuViewModel
                {
                   Option = Option.RunNewSafetyCheck,
                   Text = "Run Safety Check"
                },
            };
        }

        private void DoLogoutCommand()
        {
            Mvx.Resolve<IUserInteraction>().Confirm("Are you sure you want to log out?", isConfirmed =>
            {
                if (isConfirmed)
                {
                    _navigationService.Logout_Action(null);
                }
            }, "Logout", "Logout", "Cancel");
        }

        private void DoSelectMenuItemCommand(MenuViewModel item)
        {

            switch (item.Option)
            {
                case Option.Camera:
                    this.ShowViewModel<CameraViewModel>();
                    break;
                case Option.ViewSafetyCheck:
                    this.ShowViewModel<DisplaySafetyCheckViewModel>();
                    break;
                case Option.RunNewSafetyCheck:
                    this.ShowViewModel<SafetyCheckViewModel>();
                    break;
                case Option.Manifest:
                    this.ShowViewModel<ManifestViewModel>();
                    break;
                default:
                    break;
            }

            InitializeMenu();
        }

        #endregion
    }

}
