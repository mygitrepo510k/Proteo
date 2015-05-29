using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class VehicleListViewModel
        : BaseFragmentViewModel
    {
        private Services.IGatewayService _gatewayService;
        private IEnumerable<Vehicle> _originalVehicleList;

        private readonly ICurrentDriverRepository _currentDriverRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IToast _toast;
        private readonly IReachability _reachability;
        private readonly IInfoService _infoService;
        private readonly INavigationService _navigationService;
        private readonly ILoggingService _loggingService;

        public VehicleListViewModel(
            IGatewayService gatewayService, 
            IVehicleRepository vehicleRepository, 
            IReachability reachabibilty,
            IToast toast, 
            IInfoService infoService, 
            INavigationService navigationService, 
            ICurrentDriverRepository currentDriverRepository,
            ILoggingService loggingService)
        {
            _toast = toast;
            _reachability = reachabibilty;

            _gatewayService = gatewayService;
            _infoService = infoService;
            _navigationService = navigationService;
            _loggingService = loggingService;

            _currentDriverRepository = currentDriverRepository;
            _vehicleRepository = vehicleRepository;
            Vehicles = _originalVehicleList = _vehicleRepository.GetAll();
            _vehicleListCount = FilteredVehicleCount;

            LastVehicleSelect();
        }

        public string VehicleSelectText
        {
            get { return "Select vehicle - Showing " + FilteredVehicleCount + " of " + VehicleListCount; }
        }

        public override string FragmentTitle
        {
            get { return "Vehicle"; }
        }

        private int _vehicleListCount;
        public int VehicleListCount
        {
            get { return _vehicleListCount; }
            private set { _vehicleListCount = value; }
        }

        public int FilteredVehicleCount
        {
            get { return Vehicles.ToList().Count; }
        }

        private IEnumerable<Vehicle> _vehicles;
        public IEnumerable<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; RaisePropertyChanged(() => Vehicles); }
        }

        public void ShowTrailerScreen(Vehicle vehicle)
        {
            _infoService.LoggedInDriver.LastVehicleID = vehicle.ID;
            _infoService.CurrentVehicle = vehicle;
            _navigationService.MoveToNext();
        }

        private void LastVehicleSelect()
        {
            var currentDriver = _currentDriverRepository.GetByID(_infoService.LoggedInDriver.ID);

            if (currentDriver == null)
                return;

            var lastVehicleID = currentDriver.LastVehicleID;

            if (lastVehicleID == null)
                return;

            var vehicle = _vehicleRepository.GetByID(lastVehicleID);

            if (vehicle == null)
                return;

            Mvx.Resolve<ICustomUserInteraction>().Confirm((vehicle.Registration), isConfirmed =>
            {
                if (isConfirmed)
                {
                    ShowTrailerScreen(vehicle);
                }
            }, "Last Used Vehicle", "Confirm");
        }

        private MvxCommand<Vehicle> _showVehicleDetailCommand;
        public ICommand ShowVehicleDetailCommand
        {
            get
            {
                return (_showVehicleDetailCommand = _showVehicleDetailCommand ?? new MvxCommand<Vehicle>(async v => await VehicleDetailAsync(v)));
            }
        }

        private async Task VehicleDetailAsync(Vehicle vehicle)
        {
            if (await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(vehicle.Registration, "Confirm your vehicle", "Confirm"))
            {
                var newDriver = _currentDriverRepository.GetByID(_infoService.LoggedInDriver.ID);

                if (newDriver == null)
                    return;

                _currentDriverRepository.Delete(newDriver);
                newDriver.LastVehicleID = vehicle.ID;
                _currentDriverRepository.Insert(newDriver);

                ShowTrailerScreen(vehicle);
            }
        }

        private string _vehicleSearchText;
        public string VehicleSearchText
        {
            get { return _vehicleSearchText; }
            set { _vehicleSearchText = value; FilterList(); RaisePropertyChanged(() => VehicleSelectText); }
        }

        private void FilterList()
        {
            if (string.IsNullOrEmpty(VehicleSearchText))
            {
                Vehicles = _originalVehicleList;
            }
            else
            {
                Vehicles = _originalVehicleList.Where(t => t.Registration != null && t.Registration.ToUpper().Contains(VehicleSearchText.ToUpper()));
            }
        }

        private MvxCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get
            {
                return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(async () => await UpdateVehicleListAsync()));
            }
        }

        public async Task UpdateVehicleListAsync()
        {

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
            {
                var vehicleViews = await _gatewayService.GetVehicleViews();

                var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
                }

                var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
                var vehicles = vehiclesAndTrailers.Where(bv => !bv.IsTrailer).Select(bv => new Models.Vehicle(bv));

                if (vehicles != null)
                {
                    _vehicleRepository.DeleteAll();

                    _vehicleRepository.Insert(vehicles);

                    Vehicles = _originalVehicleList = _vehicleRepository.GetAll();

                    //Recalls the filter text if there is text in the search field.
                    if (VehicleSearchText != null)
                    {
                        FilterList();
                    }
                }
            }
        }
    }
}
