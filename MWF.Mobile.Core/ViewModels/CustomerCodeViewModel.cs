using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
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
        private readonly IVehicleViewRepository _vehicleViewRepository;
        private readonly IVerbProfileRepository _verbProfileRepository;

        public CustomerCodeViewModel(Services.IGatewayService gatewayService, IReachability reachability, Services.IDataService dataService)
        {
            _gatewayService = gatewayService;
            _dataService = dataService;
            _reachability = reachability;
            _applicationProfileRepository = Mvx.Resolve<IApplicationProfileRepository>();
            _customerRepository = Mvx.Resolve<ICustomerRepository>();
            _deviceRepository = Mvx.Resolve<IDeviceRepository>();
            _driverRepository = Mvx.Resolve<IDriverRepository>();
            _safetyProfileRepository = Mvx.Resolve<ISafetyProfileRepository>();
            _vehicleRepository = Mvx.Resolve<IVehicleRepository>();
            _vehicleViewRepository = Mvx.Resolve<IVehicleViewRepository>();
            _verbProfileRepository = Mvx.Resolve<IVerbProfileRepository>();
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
            // TODO: Get verb profile titles from config or somewhere?
            var verbProfileTitles = new[] { "Palletforce", "Cancel", "Complete", "Suspend" };
            var device = await _gatewayService.GetDevice();               
            //var applicationProfile = await _gatewayService.GetApplicationProfile();
            var drivers = (await _gatewayService.GetDrivers()).ToList();
            var vehicleViews = (await _gatewayService.GetVehicleViews()).ToList();            
            var safetyProfiles = (await _gatewayService.GetSafetyProfiles()).ToList();
            var vehicles = vehicleViews.Select(vv => vv.Title).ToDictionary(vvt => vvt, async vvt => await _gatewayService.GetVehicles(vvt));       
            var verbProfiles = verbProfileTitles.Select(async vpt => await _gatewayService.GetVerbProfile(vpt)).ToList();     

            // write all this retrieved data to the database
            _dataService.RunInTransaction(() =>
            {
                _deviceRepository.Insert(device);
                //_verbProfileRepository.Insert(verbProfiles); 
                //_applicationProfileRepository.Insert(applicationProfile);
                _driverRepository.Insert(drivers);
                _vehicleViewRepository.Insert(vehicleViews);
                //_vehicleRepository.Insert(vehicles);
                _safetyProfileRepository.Insert(safetyProfiles);
            });


            //TODO: call fwRegisterDevice - what does this actually do?

            return true;
        }

    }
}
