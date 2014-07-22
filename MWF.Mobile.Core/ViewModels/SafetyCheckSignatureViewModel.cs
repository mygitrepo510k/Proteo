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
            VehicleRegistration = startupInfoService.Vehicle.Registration;
            TrailerRef = startupInfoService.Trailer.Registration;

            //TODO: retrieve this data MWF Mobile Config repository - Luke is currently implementing this
            Preamble = "- preamble text goes here -";
            Postamble = "- postamble text goes here -";
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

        private string _preamble;
        public string Preamble
        {
            get { return _preamble; }
            set { _preamble = value; RaisePropertyChanged(() => Preamble); }
        }

        private string _postamble;
        public string Postamble
        {
            get { return _postamble; }
            set { _postamble = value; RaisePropertyChanged(() => Postamble); }
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
                _userInteraction.Alert("Signature is required");

            // Retrieve the vehicle and trailer safety check data from the startup info service
            var vehicleSafetyCheckData = _startupInfoService.VehicleSafetyCheckData;
            var trailerSafetyCheckData = _startupInfoService.TrailerSafetyCheckData;

            // Set the signature on both
            var signature = new Models.Signature { EncodedImage = this.SignatureEncodedImage };
            vehicleSafetyCheckData.Signature = trailerSafetyCheckData.Signature = signature;

            // Add the safety checks to the gateway queue
            var safetyCheckData = new[] { vehicleSafetyCheckData, trailerSafetyCheckData };
            var actions = safetyCheckData.Select(scd => new Models.GatewayServiceRequest.Action<Models.SafetyCheckData> { Command = "fwSetSafetyCheckData", Data = scd });
            _gatewayQueuedService.AddToQueue(actions);

            // The startup process is now complete - redirect to the main view
            ShowViewModel<MainViewModel>();
        }

    }

}
