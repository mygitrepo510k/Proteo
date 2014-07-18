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

        private readonly IUserInteraction _userInteraction = null;

        public SafetyCheckSignatureViewModel(IUserInteraction userInteraction)
        {
            _userInteraction = userInteraction;
            DriverName = "Tim Page";
            VehicleRegistration = "Y854 HVW";
            TrailerRef = "none";
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
            get { return "Done"; }
        }

        private Models.SignatureImage _signature;
        public Models.SignatureImage Signature
        {
            get { return _signature; }
            set { _signature = value; RaisePropertyChanged(() => Signature); }
        }

        private MvxCommand _doneCommand;
        public System.Windows.Input.ICommand DoneCommand
        {
            get { return (_doneCommand = _doneCommand ?? new MvxCommand(() => Done())); }
        }

        private void Done()
        {
            if (!Signature.Points.Any())
                _userInteraction.Alert("Signature is required");

            var encodedSignature = this.Signature.ToBlueSphereFormat();

            //TODO: add signature to safety check, queue to send to bluesphere and redirect to the main activity
        }

    }

}
