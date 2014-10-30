using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Services;
using Chance.MvvmCross.Plugins.UserInteraction;

namespace MWF.Mobile.Core.ViewModels
{

    public class SafetyCheckSignatureViewModel : BaseFragmentViewModel
    {

        private readonly Services.IStartupService _startupService = null;
        private readonly Services.IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IUserInteraction _userInteraction = null;
        private readonly INavigationService _navigationService;

        IEnumerable<Models.SafetyCheckData> _safetyCheckData;

        public SafetyCheckSignatureViewModel(Services.IStartupService startupService, Services.IGatewayQueuedService gatewayQueuedService, IUserInteraction userInteraction, Repositories.IRepositories repositories, INavigationService navigationService)
        {
            _startupService = startupService;
            _gatewayQueuedService = gatewayQueuedService;
            _userInteraction = userInteraction;
            _navigationService = navigationService;

            // Retrieve the vehicle and trailer safety check data from the startup info service
            _safetyCheckData = _startupService.GetCurrentSafetyCheckData();
            
            if (!_safetyCheckData.Any())
                throw new Exception("Invalid application state - signature screen should not be displayed in cases where there are no safety checks.");

            var combinedOverallStatus = Models.SafetyCheckData.GetOverallStatus(_safetyCheckData.Select(scd => scd.GetOverallStatus()));

            if (combinedOverallStatus == Enums.SafetyCheckStatus.NotSet)
                throw new Exception("Cannot proceed to safety check signature screen because the safety check hasn't been completed");

            DriverName = startupService.LoggedInDriver.DisplayName;
            VehicleRegistration = startupService.CurrentVehicle.Registration;
            TrailerRef = startupService.CurrentTrailer == null ? "- no trailer -" : startupService.CurrentTrailer.Registration;

            var config = repositories.ConfigRepository.Get();

            switch (combinedOverallStatus)
            {
                case Enums.SafetyCheckStatus.Failed:
                    this.ConfirmationText = config.SafetyCheckFailText;
                    break;
                case Enums.SafetyCheckStatus.DiscretionaryPass:
                    this.ConfirmationText = config.SafetyCheckDiscretionaryText;
                    break;
                case Enums.SafetyCheckStatus.Passed:
                    this.ConfirmationText = config.SafetyCheckPassText;
                    break;
                default:
                    throw new Exception("Unexpected safety check status");
            }
        }

        private string _driverName;
        public string DriverName
        {
            get { return _driverName; }
            set { _driverName = value; RaisePropertyChanged(() => DriverName); }
        }

        private string _vehicleRegistration;
        public string VehicleRegistration
        {
            get { return _vehicleRegistration; }
            set { _vehicleRegistration = value; RaisePropertyChanged(() => VehicleRegistration); }
        }

        private string _trailerRef;
        public string TrailerRef
        {
            get { return _trailerRef; }
            set { _trailerRef = value; RaisePropertyChanged(() => TrailerRef); }
        }

        private string _confirmationText;
        public string ConfirmationText
        {
            get { return _confirmationText; }
            set { _confirmationText = value; RaisePropertyChanged(() => ConfirmationText); }
        }

        public string DoneLabel
        {
            get { return "Accept"; }
        }

        private string _signatureEncodedImage;
        public string SignatureEncodedImage
        {
            get { return _signatureEncodedImage; }
            set { _signatureEncodedImage = value; RaisePropertyChanged(() => SignatureEncodedImage); }
        }

        private MvxCommand _doneCommand;
        public System.Windows.Input.ICommand DoneCommand
        {
            get { return (_doneCommand = _doneCommand ?? new MvxCommand(() => Done())); }
        }

        private void Done()
        {
            if (string.IsNullOrWhiteSpace(SignatureEncodedImage))
            {
                _userInteraction.Alert("A signature is required to complete a safety check");
                return;
            }

            // Set the signature on the vehicle and trailer safety checks
            foreach (var safetyCheckData in _safetyCheckData)
            {
                safetyCheckData.Signature = new Models.Signature { EncodedImage = this.SignatureEncodedImage };
            }

            // Complete the startup process
            _startupService.Commit();
            _startupService.Logon();
            _navigationService.MoveToNext();
        }

        public override string FragmentTitle
        {
            get { return "Signature"; }
        }
    }

}
