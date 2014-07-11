﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
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
        :MvxViewModel
    {

        public VehicleListViewModel(IVehicleExtractService service)
        {
            //Just filling the model with 10 random Vehicles
            var newList = new List<Vehicle>();
            for (var i = 0; i < 10; i++)
            {
                newList.Add(service.ExtractVehicle());
            }
            Vehicles = newList;
        }

        private List<Vehicle> _vehicles;
        public List<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; RaisePropertyChanged(() => Vehicles); }
        }

        private MvxCommand<Vehicle> _showVehicleDetailCommand;
        public ICommand ShowVehicleDetailCommand
        {
            get
            {
                return(_showVehicleDetailCommand = _showVehicleDetailCommand ?? new MvxCommand<Vehicle>(v => VehicleDetail(v)));
            }
        }

        private void VehicleDetail(Vehicle vehicle)
        {
            Mvx.Resolve<IUserInteraction>().Confirm("Vehicle ID: " + vehicle.ID, isConfirmed => {
                if (isConfirmed)
                {
                    ShowViewModel<TrailerSelectionViewModel>(new TrailerSelectionViewModel.Nav { ID = vehicle.ID });
                }
            },"Is this your vehicle?");
            
        }  
    }
}
