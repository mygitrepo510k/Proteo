using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;

namespace MWF.Mobile.Core.ViewModels
{

    public class SafetyCheckSignatureViewModel : MvxViewModel
    {

        private readonly Services.IStartupInfoService _startupInfoService = null;
        private readonly Services.IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IUserInteraction _userInteraction = null;

        public SafetyCheckSignatureViewModel(Services.IStartupInfoService startupInfoService, Services.IGatewayQueuedService gatewayQueuedService, IUserInteraction userInteraction)
        {
            _startupInfoService = startupInfoService;
            _gatewayQueuedService = gatewayQueuedService;
            _userInteraction = userInteraction;

            DriverName = startupInfoService.LoggedInDriver.DisplayName;
            VehicleRegistration = startupInfoService.CurrentVehicle.Registration;
            TrailerRef = startupInfoService.CurrentTrailer == null ? "- no trailer -" : startupInfoService.CurrentTrailer.Registration;

            //TODO: retrieve the relevant message from the MWF Mobile Config repository - Luke is currently implementing this
            ConfirmationText = "I confirm that this vehicle is NOT safe or roadworthy - Please call the traffic office on 0845 644 3750 to inform them of these fault(s).";
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
                _userInteraction.Alert("Signature is required");
                return;
            }

            // Retrieve the vehicle and trailer safety check data from the startup info service
            var safetyCheckDataList = new List<Models.SafetyCheckData>(2);

            if (_startupInfoService.CurrentVehicleSafetyCheckData != null)
                safetyCheckDataList.Add(_startupInfoService.CurrentVehicleSafetyCheckData);

            if (_startupInfoService.CurrentTrailerSafetyCheckData != null)
                safetyCheckDataList.Add(_startupInfoService.CurrentTrailerSafetyCheckData);

            // Set the signature on both
            var signature = new Models.Signature { EncodedImage = this.SignatureEncodedImage };

            foreach (var safetyCheckData in safetyCheckDataList)
            {
                safetyCheckData.Signature = signature;
            }

            // Add the safety checks to the gateway queue
            var actions = safetyCheckDataList.Select(scd => new Models.GatewayServiceRequest.Action<Models.SafetyCheckData> { Command = "fwSetSafetyCheckData", Data = scd });
            _gatewayQueuedService.AddToQueue(actions);

            // The startup process is now complete - redirect to the main view
            ShowViewModel<MainViewModel>();
        }

    }

}
