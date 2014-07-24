using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class OdometerViewModel : BaseFragmentViewModel
    {
        private IStartupService _startupService;
        public OdometerViewModel(IStartupService startupService)
        {
            _startupService = startupService;
        }

        public override string FragmentTitle
        {
            get { return "Odometer"; }
        }

        public string OdometerLabel
        {
            get { return "Odometer"; }
        }

        private string _odometerValue;
        public string OdometerValue
        {
            get { return _odometerValue; }
            set { _odometerValue = value; RaisePropertyChanged(() => OdometerValue); }
        }

        public string OdometerButtonLabel
        {
            get { return "Submit"; }
        }

        private MvxCommand _storeCommand;
        public System.Windows.Input.ICommand StoreCommand
        {
            get
            {
                _storeCommand = _storeCommand ?? new MvxCommand(DoStoreCommand);
                return _storeCommand;
            }
        }


        public string Registration
        {
            get { return _startupService.CurrentVehicle.Registration; }
        }
        

        private void DoStoreCommand()
        {
            int odometerValue = int.Parse(OdometerValue);
            _startupService.Mileage = odometerValue;

            ShowViewModel<SafetyCheckSignatureViewModel>();
        }



    }
}
