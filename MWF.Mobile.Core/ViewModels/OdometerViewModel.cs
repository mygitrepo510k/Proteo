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
        private readonly IStartupService _startupService;
        private readonly INavigationService _navigationService;

        public OdometerViewModel(IStartupService startupService, INavigationService navigationService)
        {
            _startupService = startupService;
            _navigationService = navigationService;
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
            get { return "Done"; }
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

            _navigationService.MoveToNext();
        }



    }
}
