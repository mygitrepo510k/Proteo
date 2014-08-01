using System;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.Portable;

namespace MWF.Mobile.Core.ViewModels
{

    public class PasscodeViewModel
        : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICurrentDriverRepository _currentDriverRepository = null;
        private readonly IAuthenticationService _authenticationService = null;
        private readonly IStartupService _startupService = null;
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;
        private bool _isBusy = false;

        public PasscodeViewModel(IAuthenticationService authenticationService, IStartupService startupService,
                        ICloseApplication closeApplication, IRepositories repositories, INavigationService navigationService)
        {
            _authenticationService = authenticationService;
            _startupService = startupService;
            _navigationService = navigationService;

            _currentDriverRepository = repositories.CurrentDriverRepository;
            _closeApplication = closeApplication;
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string PasscodeLabel
        {
            get { return "Driver Passcode"; }
        }

        public string PasscodeButtonLabel
        {
            get { return "Submit"; }
        }

        public string ProgressTitle
        {
            get { return "Checking Passcode..."; }
        }

        public string ProgressMessage
        {
            get { return "Your driver passcode is being checked. This can take up to 5 minutes."; }
        }

        private string _passcode = null;
        public string Passcode
        {
            get { return _passcode; }
            set { _passcode = value; RaisePropertyChanged(() => Passcode); }
        }

        private bool _isAuthenticating = false;
        public bool IsAuthenticating
        {
            get { return _isAuthenticating; }
            set { _isAuthenticating = value; RaisePropertyChanged(() => IsAuthenticating); }
        }

        private MvxCommand _loginCommand;
        public System.Windows.Input.ICommand LoginCommand
        {
            get { return (_loginCommand = _loginCommand ?? new MvxCommand(async () => await LoginAsync())); }
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Passcode))
            {
                //TODO: probably should additionally implement presentation layer required field validation so we don't even get this far.
                await Mvx.Resolve<IUserInteraction>().AlertAsync("To login, submit a passcode");
                return;
            }

            await Authenticate();
        }

        #region Private Methods

        private async Task Authenticate()
        {
            IsBusy = true;
            AuthenticationResult result;

            try
            {
                result = await _authenticationService.AuthenticateAsync(this.Passcode);

                if (result.Success)
                {
                    _startupService.LoggedInDriver = result.Driver;

                    if (_currentDriverRepository.GetByID(_startupService.LoggedInDriver.ID) == null)
                    {
                        CurrentDriver newDriver = new CurrentDriver();
                        newDriver.ID = _startupService.LoggedInDriver.ID;
                        _currentDriverRepository.Insert(newDriver);
                    }

                    // Start the gateway queue timer which will cause submission of any queued data to the MWF Mobile gateway service on a repeat basis
                    _startupService.StartGatewayQueueTimer();
                    
                    _navigationService.MoveToNext();
                }
            }
            catch
            {
                // TODO: log the exception to be picked up by bluesphere
                result = new AuthenticationResult() { AuthenticationFailedMessage = "Unable to check your passcode.", Success = false };
            }
            finally
            {
                // clear the passcode
                this.Passcode = string.Empty;
                IsBusy = false;
            }

            // Let the user know
            if (!result.Success) await Mvx.Resolve<IUserInteraction>().AlertAsync(result.AuthenticationFailedMessage);
        }

        #endregion

        public override string FragmentTitle
        {
            get { return "Passcode"; }
        }

        public async Task<bool> OnBackButtonPressed()
        {

            bool continueWithBackPress = await Mvx.Resolve<IUserInteraction>().ConfirmAsync("Do you wish to leave?", "Changes will be lost!");

            if (continueWithBackPress)
            {
               _closeApplication.CloseApp();
            }

            return false;
        }
    }

}
