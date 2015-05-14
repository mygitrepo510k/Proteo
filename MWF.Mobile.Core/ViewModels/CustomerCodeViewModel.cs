using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Helpers;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using Chance.MvvmCross.Plugins.UserInteraction;
using MWF.Mobile.Core.Repositories.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{

    public class CustomerCodeViewModel : BaseFragmentViewModel
    {

        private readonly Services.IGatewayService _gatewayService;
        private readonly Services.IDataService _dataService;
        private readonly IReachability _reachability;
        private readonly IUserInteraction _userInteraction;
        private readonly INavigationService _navigationService;

        private readonly IApplicationProfileRepository _applicationProfileRepository;
        private readonly ICustomerRepository _customerRepository; 
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ISafetyProfileRepository _safetyProfileRepository;
        private readonly ITrailerRepository _trailerRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVerbProfileRepository _verbProfileRepository;
        private readonly IConfigRepository _configRepository;

        public CustomerCodeViewModel(IGatewayService gatewayService, IReachability reachability, IDataService dataService, IRepositories repositories, IUserInteraction userInteraction, INavigationService navigationService)
        {
            _gatewayService = gatewayService;
            _dataService = dataService;
            _reachability = reachability;
            _userInteraction = userInteraction;
            _navigationService = navigationService;

            _applicationProfileRepository = repositories.ApplicationRepository;
            _customerRepository = repositories.CustomerRepository;
            _deviceRepository = repositories.DeviceRepository;
            _driverRepository = repositories.DriverRepository;
            _safetyProfileRepository = repositories.SafetyProfileRepository;
            _trailerRepository = repositories.TrailerRepository;
            _vehicleRepository = repositories.VehicleRepository;
            _verbProfileRepository = repositories.VerbProfileRepository;
            _configRepository = repositories.ConfigRepository;

#if DEBUG
            //Firmin
            //_userInteraction.Confirm("DEBUGGING: use the Firmin customer code?", (bool ok) => { if (ok) this.CustomerCode = "A2A67DE7-DC95-49D9-BF53-34829CF865C9"; });
            //Demo.ProteoMobile Customer Code
            //_userInteraction.Confirm("DEBUGGING: use the MWF Dev customer code?", (bool ok) => { if (ok) this.CustomerCode = "C697166B-2E1B-45B0-8F77-270C4EADC031"; });
            //Demo.ProteoEnterprise Customer Code
            //_userInteraction.Confirm("DEBUGGING: use the Proteo Demo customer code?", (bool ok) => { if (ok) this.CustomerCode = "DB78C027-CA85-4DF3-A25A-760562C4EEB5"; });
            //Fagan & Whalley
            _userInteraction.Confirm("DEBUGGING: use the Fagan & Whalley customer code?", (bool ok) => { if (ok) this.CustomerCode = "B448BEF1-9320-4AAB-A529-06605A288A96"; });
#endif
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
            get { return "Submit"; }
        }

        public string CustomerCodeLabel
        {
            get { return "Customer Code"; }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string ProgressTitle
        {
            get { return "Downloading Data...";  }
        }

        public string ProgressMessage
        {
            get { return "Your customer code is being checked to set-up your device. This can take up to 5 minutes.";  }
        }

        private MvxCommand _enterCodeCommand;
        public System.Windows.Input.ICommand EnterCodeCommand
        {
            get
            {
                _enterCodeCommand = _enterCodeCommand ?? new MvxCommand(async () => await EnterCodeAsync());
                return _enterCodeCommand;
            }
        }

        private string _errorMessage;

        private string _unexpectedErrorMessage = "Unfortunately, there was a problem setting up your device, try restarting the device and try again.";
        private async Task EnterCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(this.CustomerCode))
            {
                //TODO: probably should additionally implement presentation layer required field validation so we don't even get this far.
                await _userInteraction.AlertAsync("To register this device, submit a customer code");
                return;
            }

            if (!_reachability.IsConnected())
            {
                await _userInteraction.AlertAsync("To set-up this device, a connection to the internet is required");
            }
            else
            {
                bool success = false;

                this.IsBusy = true;

                try
                {
                    success = await this.SetupDevice();
                }
                catch(Exception e)
                {
                    //TODO: save to unhandled exceptions log
                    success = false;
                    _errorMessage = _unexpectedErrorMessage;
                }

                this.IsBusy = false;

                if (success) _navigationService.MoveToNext();
                else await _userInteraction.AlertAsync(_errorMessage);

            }
        }


        // returns false if the customer code is not known
        // throws exceptions if the web services or db inserts fail
        private async Task<bool> SetupDevice()
        {

            if (!await _gatewayService.CreateDevice())
            {
                _errorMessage = _unexpectedErrorMessage;
                return false;
            }
                

            var device = await _gatewayService.GetDevice(CustomerCode);
            if (device == null)
            {
                _errorMessage = "The customer passcode you submitted doesn't exist, check the passcode and try again.";
                return false;
            }
            var config = await _gatewayService.GetConfig();
            var applicationProfile = await _gatewayService.GetApplicationProfile();
            var drivers = await _gatewayService.GetDrivers();
            var vehicleViews = await _gatewayService.GetVehicleViews();
            var safetyProfiles = await _gatewayService.GetSafetyProfiles();

            var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

            foreach (var vehicleView in vehicleViews)
            {
                vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
            }

            var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
            var vehicles = vehiclesAndTrailers.Where(bv => !bv.IsTrailer).Select(bv => new Models.Vehicle(bv));
            var trailers = vehiclesAndTrailers.Where(bv => bv.IsTrailer).Select(bv => new Models.Trailer(bv));

            //TODO: I'm not sure whether verb profiles will be used anywhere within this app, or whether this is a hangover from the Tough Touch implementation? If not used on completion of the project then remove from here and local db.
            // TODO: Get verb profile titles from config or somewhere?
            var verbProfileTitles = new[] { "Palletforce", "Cancel", "Complete", "Suspend" };
            var verbProfiles = new List<Models.VerbProfile>(verbProfileTitles.Count());
            
            foreach (var verbProfileTitle in verbProfileTitles)
            {
                verbProfiles.Add(await _gatewayService.GetVerbProfile(verbProfileTitle));
            }          

            _dataService.RunInTransaction( connection =>
            {
                //TODO: Store the customer title? Need to get the customer title from somewhere.
                _customerRepository.Insert(new Customer() { ID = new Guid(), CustomerCode = CustomerCode }, connection);
                _deviceRepository.Insert(device, connection);
                _verbProfileRepository.Insert(verbProfiles, connection);
                _applicationProfileRepository.Insert(applicationProfile, connection);
                _driverRepository.Insert(drivers, connection);
                //TODO: relate Vehicles to VehicleViews?  Are VehicleViews actually used for anything within the app?
                _vehicleRepository.Insert(vehicles, connection);
                _trailerRepository.Insert(trailers, connection);
                _safetyProfileRepository.Insert(safetyProfiles, connection);
                _configRepository.Insert(config, connection);
            });



            //TODO: call fwRegisterDevice - what does this actually do?

            return true;
        }


    }
}
