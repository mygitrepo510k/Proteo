using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
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

namespace MWF.Mobile.Core.ViewModels
{

    public class VehicleListViewModel
        : MvxViewModel
    {
        private Services.IGatewayService _gatewayService;
        private IEnumerable<Vehicle> _originalVehicleList;
        private IVehicleRepository _vehicleRepository;


        public string VehicleSelectText
        {
            get { return "Please select a trailer."; }
        }

        public VehicleListViewModel(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
            Vehicles = _originalVehicleList = _vehicleRepository.GetAll();  
        }
        
        private IEnumerable<Vehicle> _vehicles;
        public IEnumerable<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; RaisePropertyChanged(() => Vehicles); }
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
            Mvx.Resolve<IUserInteraction>().Confirm("Registration: " + vehicle.Registration, isConfirmed =>
            {
                if (isConfirmed)
                {
                    ShowViewModel<TrailerSelectionViewModel>(new TrailerSelectionViewModel.Nav { ID = vehicle.ID });
                }
            }, "Please confirm your vehicle");

        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); FilterList(); }
        }


        private void FilterList()
        {
            Vehicles = _originalVehicleList.Where(v => v.ID.ToString().Contains(SearchText));
        }

        private MvxCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get
            {

                return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(() => updateVehicleList()));
            }
        }

        public async void updateVehicleList()
        {
            
            _gatewayService = Mvx.Resolve<IGatewayService>();
            var vehicleViews = await _gatewayService.GetVehicleViews();

            var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

            foreach (var vehicleView in vehicleViews)
            {
                vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
            }

            var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
            var vehicles = vehiclesAndTrailers.Where(bv => !bv.IsTrailer).Select(bv => new Models.Vehicle(bv));
            
            var rows = _vehicleRepository.GetAll().ToList();

            foreach (var row in rows)
            {
                _vehicleRepository.Delete(row);
            }

            _vehicleRepository.Insert(vehicles);

            Vehicles = _originalVehicleList = _vehicleRepository.GetAll();  
        }
    }  
}
