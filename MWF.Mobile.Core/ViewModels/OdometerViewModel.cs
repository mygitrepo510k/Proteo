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
        private IStartupInfoService _startupInfoService;
        public OdometerViewModel(IStartupInfoService startupInfoService)
        {
            _startupInfoService = startupInfoService;
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
            get { return _startupInfoService.CurrentVehicle.Registration; }
        }
        

        private void DoStoreCommand()
        {
            int odometerValue = int.Parse(OdometerValue);
            _startupInfoService.Mileage = odometerValue;

            //TODO navigate to safety check acceptance screen
            
        }



    }
}
