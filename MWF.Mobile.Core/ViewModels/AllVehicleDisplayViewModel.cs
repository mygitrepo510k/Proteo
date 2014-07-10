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
    public class AllVehicleDisplayViewModel 
        :MvxViewModel
    {

        public AllVehicleDisplayViewModel(IVehicleExtractService service)
        {
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

        public ICommand ShowDetailCommand
        {
            get
            {
                return new MvxCommand<Vehicle>(item => ShowViewModel<VehicleDetailViewModel>(new VehicleDetailViewModel.Nav() { ID = item.ID }));
            }
        }
        
    }
}
