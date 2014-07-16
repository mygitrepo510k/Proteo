using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
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
        private IEnumerable<Vehicle> _originalVehicleList;

        public VehicleListViewModel(IVehicleRepository vehicleRepository)
        {
            Vehicles = _originalVehicleList = vehicleRepository.GetAll();
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
            Mvx.Resolve<IUserInteraction>().Confirm("Vehicle ID: " + vehicle.ID, isConfirmed =>
            {
                if (isConfirmed)
                {
                    ShowViewModel<TrailerSelectionViewModel>(new TrailerSelectionViewModel.Nav { ID = vehicle.ID });
                }
            }, "Is this your vehicle?");

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
        
    }  


}
