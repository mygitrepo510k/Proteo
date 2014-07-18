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
using MWF.Mobile.Core.Portable;

namespace MWF.Mobile.Core.ViewModels
{

    public class VehicleListViewModel
        : MvxViewModel
    {
        private Services.IGatewayService _gatewayService;
        private IEnumerable<Vehicle> _originalVehicleList;

        private readonly IVehicleRepository _vehicleRepository;
        private readonly IToast _toast;
        private readonly IReachability _reachability;


        public string VehicleSelectText
        {
            get { return "Please select a trailer."; }
        }

        public VehicleListViewModel(IVehicleRepository vehicleRepository, IReachability reachabibilty, IToast toast)
        {
            _toast = toast;
            _reachability = reachabibilty;
            
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

        //This is method associated with the search button in the action bar.
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); FilterList(); }
        }


        private void FilterList()
        {
            Vehicles = _originalVehicleList.Where(v => v.Registration.Contains(SearchText));
        }

        //This is method associated with the refresh button in the action bar. 
        private MvxCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get
            {

                return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(() => updateVehicleList()));
            }
        }

        public async Task updateVehicleList()
        {

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
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
