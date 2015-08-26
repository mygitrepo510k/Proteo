using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using Cirrious.CrossCore;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{

    public class DiagnosticsViewModel : BaseFragmentViewModel, IModalViewModel<bool>
    {

        private readonly IDataService _dataService;
        private readonly IReachability _reachability;
        private readonly ICustomUserInteraction _userInteraction;
        private readonly INavigationService _navigationService;
        private readonly IDiagnosticsService _diagnosticsService;

        private readonly IApplicationProfileRepository _applicationProfileRepository;
        private readonly ICustomerRepository _customerRepository; 
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ISafetyProfileRepository _safetyProfileRepository;
        private readonly ITrailerRepository _trailerRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVerbProfileRepository _verbProfileRepository;
        private readonly IConfigRepository _configRepository;

        public DiagnosticsViewModel(IReachability reachability, IDataService dataService, IRepositories repositories, ICustomUserInteraction userInteraction, INavigationService navigationService, IDiagnosticsService diagnosticsService)
        {
            _dataService = dataService;
            _reachability = reachability;
            _userInteraction = userInteraction;
            _navigationService = navigationService;
            _diagnosticsService = diagnosticsService;

            _applicationProfileRepository = repositories.ApplicationRepository;
            _configRepository = repositories.ConfigRepository;


        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string FragmentTitle { get { return "Customer Code"; } }

        private string _customerCode = string.Empty;
        public string CustomerCode
        {
            get { return _customerCode; }
            set { _customerCode = value; RaisePropertyChanged(() => CustomerCode); }
        }

        public string EnterButtonLabel
        {
            get { return "Send"; }
        }

        public string DiagnosticsMessageLabel
        {
            get { return "Push the Send button to send diagnostic information to Proteo"; }
        }

        public string VersionText
        {
            get { return string.Format("Version: {0}           DeviceID: {1}",Mvx.Resolve<IDeviceInfo>().SoftwareVersion, Mvx.Resolve<IDeviceInfo>().GetDeviceIdentifier()); }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string ProgressTitle
        {
            get { return "Uploading Data...";  }
        }

        public string ProgressMessage
        {
            get { return "The data is being uploaded please wait.";  }
        }

        private MvxCommand _sendDiagnostocsCommand;
        public System.Windows.Input.ICommand SendDiagnosticsCommand
        {
            get
            {
                _sendDiagnostocsCommand = _sendDiagnostocsCommand ?? new MvxCommand(async () => await UploadDiagnosticsAsync());
                return _sendDiagnostocsCommand;
            }
        }

      
        private string _errorMessage;
        private string _unexpectedErrorMessage = "Unfortunately, there was a problem setting up your device, try restarting the device and try again.";


        private async Task UploadDiagnosticsAsync()
        {
            if (!_reachability.IsConnected())
            {
                await _userInteraction.AlertAsync("You need a connection to the internet to submit diagnostics.");
            }
            else
            {
                this.IsBusy = true;
                bool success = false;
                try
                {
                    success = this.UploadDiagnostics();
                }
                catch(Exception ex)
                {
                    success = false;
                    _errorMessage = _unexpectedErrorMessage;
                }

                this.IsBusy = false;

                if (success)
                {
                    _userInteraction.Alert("The support information has been uploaded", null, "Support");
                    _navigationService.MoveToNext();
                }
                else await _userInteraction.AlertAsync(_errorMessage);
            }
        }
     

        private bool UploadDiagnostics()
        {
            // get the database
            var dbFile = _dataService.GetDBConnection();
            var path = dbFile.DatabasePath;
           
           var success =  _diagnosticsService.UploadDiagnostics(path);

            return success;

        }

        public void Cancel()
        {
           // throw new NotImplementedException();
        }

        public void ReturnResult(bool result)
        {
           // throw new NotImplementedException();
        }

        // returns false if the customer code is not known
        public Guid MessageId { get; set; }


    }
}
