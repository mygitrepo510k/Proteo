using System;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.ViewModels
{

    public class PasscodeViewModel
        : MvxViewModel
    {
        private readonly ICurrentDriverRepository _currentDriverRepository = null;
        private readonly IAuthenticationService _authenticationService = null;
        private readonly IStartupInfoService _startupInfoService = null;
        private bool _isBusy = false;

        public PasscodeViewModel(IAuthenticationService authenticationService, IStartupInfoService startupInfoService
            , ICurrentDriverRepository currentDriverRepository)
        {
            _authenticationService = authenticationService;
            _startupInfoService = startupInfoService;

            _currentDriverRepository = currentDriverRepository;
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
            get { return "Please wait while we check your passcode..."; }
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
                await Mvx.Resolve<IUserInteraction>().AlertAsync("Passcode cannot be blank");
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
                    _startupInfoService.LoggedInDriver = result.Driver;

                    if (_currentDriverRepository.GetByID(_startupInfoService.LoggedInDriver.ID) == null)
                    {
                        CurrentDriver newDriver = new CurrentDriver();
                        newDriver.ID = _startupInfoService.LoggedInDriver.ID;
                        _currentDriverRepository.Insert(newDriver);
                    }

                    ShowViewModel<VehicleListViewModel>();
                }

            }
            catch (Exception ex)
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

    }

}
