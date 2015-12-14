using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{

    public class PasscodeViewModel
        : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICurrentDriverRepository _currentDriverRepository = null;
        private readonly IAuthenticationService _authenticationService = null;
        private readonly IInfoService _infoService = null;
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;
        private readonly ILoggingService _loggingService;
        private readonly IGatewayQueuedService _gatewayQueuedService;
        private bool _isBusy = false;

        public PasscodeViewModel(
            IAuthenticationService authenticationService, 
            IInfoService infoService,
            ICloseApplication closeApplication, 
            IRepositories repositories, 
            INavigationService navigationService,
            ILoggingService loggingService,
            IGatewayQueuedService gatewayQueuedService)
        {
            _authenticationService = authenticationService;
            _infoService = infoService;
            _navigationService = navigationService;
            _loggingService = loggingService;

            _currentDriverRepository = repositories.CurrentDriverRepository;
            _closeApplication = closeApplication;
            _gatewayQueuedService = gatewayQueuedService;
        }

        #region Public Members

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

        public string VersionText
        {
            get { return string.Format("Version: {0}           DeviceID: {1}", Mvx.Resolve<IDeviceInfo>().SoftwareVersion, Mvx.Resolve<IDeviceInfo>().IMEI); }
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

        private MvxCommand _loginCommand;
        public System.Windows.Input.ICommand LoginCommand
        {
            get { return (_loginCommand = _loginCommand ?? new MvxCommand(async () => await this.LoginAsync())); }
        }

        private MvxCommand _sendDiagnosticsCommand;
        public System.Windows.Input.ICommand SendDiagnosticsCommand
        {
            get { return (_sendDiagnosticsCommand = _sendDiagnosticsCommand ?? new MvxCommand(async () => await this.SendDiagnosticsAsync()));}
        }
        public override string FragmentTitle
        {
            get { return "Passcode"; }
        }

        #endregion Public Members

        #region Private Methods

        private Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Passcode))
            {
                //TODO: probably should additionally implement presentation layer required field validation so we don't even get this far.
                return Mvx.Resolve<ICustomUserInteraction>().AlertAsync("To login, submit a passcode");
            }

            return this.AuthenticateAsync();
        }

        private Task SendDiagnosticsAsync()
        {
            NavData<object> navData = new NavData<object>();
            navData.OtherData["Diagnostics"] = true;         
            return _navigationService.MoveToNextAsync(navData);
        }

        private async Task AuthenticateAsync()
        {
            IsBusy = true;
            AuthenticationResult result;

            try
            {
                result = await _authenticationService.AuthenticateAsync(this.Passcode);

                if (result.Success)
                {
                    _infoService.LoggedInDriver = result.Driver;

                    if (await _currentDriverRepository.GetByIDAsync(_infoService.LoggedInDriver.ID) == null)
                    {
                        CurrentDriver newDriver = new CurrentDriver();
                        newDriver.ID = _infoService.LoggedInDriver.ID;
                        await _currentDriverRepository.InsertAsync(newDriver);
                    }

                    // Start the gateway queue timer which will cause submission of any queued data to the MWF Mobile gateway service on a repeat basis
                    _gatewayQueuedService.StartQueueTimer();
                    
                    await _navigationService.MoveToNextAsync();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogEventAsync(ex);
                result = new AuthenticationResult() { AuthenticationFailedMessage = "Unable to check your passcode.", Success = false };
            }
            finally
            {
                // clear the passcode
                this.Passcode = string.Empty;
                IsBusy = false;
            }

            // Let the user know
            if (!result.Success)
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync(result.AuthenticationFailedMessage);
        }

        #endregion

        #region IBackButtonHandler Implementation

        public async Task<bool> OnBackButtonPressedAsync()
        {
            var closeApp = true;

#if DEBUG
            closeApp = !await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("DEBUGGING: Return to Customer Code screen?", cancelButton: "No, close the app");
#endif

            if (closeApp)
                _closeApplication.CloseApp();
            else
                ShowViewModel<CustomerCodeViewModel>();

            return false;
        }

        #endregion IBackButtonHandler Implementation
    }

}
