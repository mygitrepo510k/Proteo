using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class PasscodeViewModel 
		: MvxViewModel
    {

        private readonly Services.IAuthenticationService _authenticationService = null;

        public PasscodeViewModel(Services.IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public string PasscodeLabel
        {
            get { return "Driver Passcode"; }
        }

        public string PasscodeButtonLabel
        {
            get { return "Submit"; }
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

            var result = await _authenticationService.AuthenticateAsync(this.Passcode);

            if (result.Success)
                ShowViewModel<VehicleListViewModel>();
            else
            { 
                await Mvx.Resolve<IUserInteraction>().AlertAsync(result.AuthenticationFailedMessage);
                // clear the passcode
                this.Passcode = string.Empty;

            }
        }

    }

}
