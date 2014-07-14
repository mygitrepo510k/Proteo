﻿using Cirrious.MvvmCross.ViewModels;
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
            Vehicles = _originalVehicleList = newList;
        }
        private List<Vehicle> _originalVehicleList;

        private IEnumerable<Vehicle> _vehicles;
        public IEnumerable<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; RaisePropertyChanged(() => Vehicles); }
        }

        public ICommand ShowVehicleDetailCommand
        {
            get
            {
                return new MvxCommand<Vehicle>(v => ShowViewModel<VehicleDetailViewModel>(new VehicleDetailViewModel.Nav { ID = v.ID }));
            }

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
