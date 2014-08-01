using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories.Interfaces;

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
        private readonly IStartupService _startupService;
        private readonly INavigationService _navigationService;

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

        public VehicleListViewModel(IGatewayService gatewayService, IVehicleRepository vehicleRepository, IReachability reachabibilty,
            IToast toast, IStartupService startupService, INavigationService navigationService, ICurrentDriverRepository currentDriverRepository)
        {
            _gatewayService = gatewayService;
            _toast = toast;
            _reachability = reachabibilty;
            _startupService = startupService;
            _navigationService = navigationService;

            _currentDriverRepository = currentDriverRepository;
            _vehicleRepository = vehicleRepository;
            Vehicles = _originalVehicleList = _vehicleRepository.GetAll();
            _vehicleListCount = FilteredVehicleCount;

            LastVehicleSelect();
        }

        private IEnumerable<Vehicle> _vehicles;
        public IEnumerable<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; RaisePropertyChanged(() => Vehicles); }
        }

        public void ShowTrailerScreen(Vehicle vehicle)
        {
            _startupService.LoggedInDriver.LastVehicleID = vehicle.ID;
            _startupService.CurrentVehicle = vehicle;
            _navigationService.MoveToNext();
        }

        public void LastVehicleSelect()
        {
            var currentDriver = _currentDriverRepository.GetByID(_startupService.LoggedInDriver.ID);

            if (currentDriver == null)
                return;

            var lastVehicleID = currentDriver.LastVehicleID;

            if (lastVehicleID == null)
                return;

            var vehicle = _vehicleRepository.GetByID(lastVehicleID);

            if (vehicle == null)
                return;

            Mvx.Resolve<IUserInteraction>().Confirm((vehicle.Registration),isConfirmed =>
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
                return (_showVehicleDetailCommand = _showVehicleDetailCommand ?? new MvxCommand<Vehicle>(v => VehicleDetail(v)));
            }
        }

        private void VehicleDetail(Vehicle vehicle)
        {

            Mvx.Resolve<IUserInteraction>().Confirm(vehicle.Registration, isConfirmed =>
            {
                if (isConfirmed)
                {
                    var newDriver = _currentDriverRepository.GetByID(_startupService.LoggedInDriver.ID);

                    if (newDriver == null)
                        return;

                    _currentDriverRepository.Delete(newDriver);
                    newDriver.LastVehicleID = vehicle.ID;
                    _currentDriverRepository.Insert(newDriver);

                    ShowTrailerScreen(vehicle);
                }
            }, "Confirm your vehicle", "Confirm");
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); FilterList(); RaisePropertyChanged(() => VehicleSelectText); }
        }

        private void FilterList()
        {
            Vehicles = _originalVehicleList.Where(v => v.Registration.ToUpper().Contains(SearchText.ToUpper()));
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
                    if (SearchText != null)
                    {
                        FilterList();
                    }
                }
            }
        }
    }
}
