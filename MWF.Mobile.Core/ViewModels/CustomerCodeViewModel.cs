using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Helpers;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using Chance.MvvmCross.Plugins.UserInteraction;

namespace MWF.Mobile.Core.ViewModels
{

    public class CustomerCodeViewModel : MvxViewModel
    {

        private readonly Services.IGatewayService _gatewayService;
        private readonly Services.IDataService _dataService;
        private readonly IReachability _reachability;
        private readonly IApplicationProfileRepository _applicationProfileRepository;
        private readonly ICustomerRepository _customerRepository; 
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ISafetyProfileRepository _safetyProfileRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVerbProfileRepository _verbProfileRepository;

        public CustomerCodeViewModel(IGatewayService gatewayService, IReachability reachability, IDataService dataService, IRepositories repositories)
        {
            _gatewayService = gatewayService;
            _dataService = dataService;
            _reachability = reachability;
            _applicationProfileRepository = repositories.ApplicationRepository;
            _customerRepository = repositories.CustomerRepository;
            _deviceRepository = repositories.DeviceRepository;
            _driverRepository = repositories.DriverRepository;
            _safetyProfileRepository = repositories.SafetyProfileRepository;
            _vehicleRepository = repositories.VehicleRepository;
            _verbProfileRepository = repositories.VerbProfileRepository;
        }

        private string _customerCode = null;
        public string CustomerCode
        {
            get { return _customerCode; }
            set { _customerCode = value; RaisePropertyChanged(() => CustomerCode); }
        }

        public string EnterButtonLabel
        {
            get { return "Save Customer Code"; }
        }

        public string CustomerCodeLabel
        {
            get { return "Please enter your Customer Code"; }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
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

        private async Task EnterCodeAsync()
        {
            if (!_reachability.IsConnected())
            {
                await Mvx.Resolve<IUserInteraction>().AlertAsync("An Internet connection is required");
            }
            else
            {
                this.IsBusy = true;
                
                if (await this.AssociateDeviceToCustomer() && await this.SetupDevice())
                {
                    //TODO: if success then save code to database
                }

                this.IsBusy = false;
            }
        }

        private async Task<bool> AssociateDeviceToCustomer()
        {
            //TODO: implement this
            return true;
        }

        private async Task<bool> SetupDevice()
        {
            var device = await _gatewayService.GetDevice();               
            var applicationProfile = await _gatewayService.GetApplicationProfile();
            var drivers = await _gatewayService.GetDrivers();
            var vehicleViews = await _gatewayService.GetVehicleViews();
            var safetyProfiles = await _gatewayService.GetSafetyProfiles();

            var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.Vehicle>>(vehicleViews.Count());

            foreach (var vehicleView in vehicleViews)
            {
                vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
            }

            var vehicles = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);

            // TODO: Get verb profile titles from config or somewhere?
            var verbProfileTitles = new[] { "Palletforce", "Cancel", "Complete", "Suspend" };
            var verbProfiles = new List<Models.VerbProfile>(verbProfileTitles.Count());
            
            foreach (var verbProfileTitle in verbProfileTitles)
            {
                verbProfiles.Add(await _gatewayService.GetVerbProfile(verbProfileTitle));
            }          


            // write all this retrieved data to the database
            _dataService.RunInTransaction(() =>
            {
                _deviceRepository.Insert(device);
                _verbProfileRepository.Insert(verbProfiles); 
                _applicationProfileRepository.Insert(applicationProfile);
                _driverRepository.Insert(drivers);
                //TODO: relate Vehicles to VehicleViews?  Are VehicleViews actually used for anything within the app?
                _vehicleRepository.Insert(vehicles);
                _safetyProfileRepository.Insert(safetyProfiles);
            });


            //TODO: call fwRegisterDevice - what does this actually do?

            return true;
        }

    }
}
