using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{

    public class TrailerSelectionViewModel
        :MvxViewModel
    {

        private Vehicle _vehicle;

        public class Nav
        {
            public Guid ID { get; set; }
        }

        public void Init(Nav nav)
        {
            Vehicle = new Vehicle
            {
                ID = nav.ID
                
            };
        }

        public Vehicle Vehicle
        {
            get { return _vehicle; }
            set { _vehicle = value; RaisePropertyChanged(() => Vehicle); }
        }

        //This is trailer selection, using object vehicle as a temp holder for now. Until the database/ models get updated.
        private List<Vehicle> _trailerList;
        public TrailerSelectionViewModel()
        {
            var newTrailerList = new List<Vehicle>();
            for (var i = 0; i < 10; i++)
            {
                Vehicle tempVehicle = new Vehicle();
                tempVehicle.Title = "Trailer " + i;
                newTrailerList.Add(tempVehicle);
            }
            Trailers = _trailerList = newTrailerList;
        }

        private IEnumerable<Vehicle> _trailers;
        public IEnumerable<Vehicle> Trailers
        {
            get { return _trailers; }
            set { _trailers = value; RaisePropertyChanged(() => Trailers); }
        }

        private MvxCommand<Vehicle> _trailerSelectorCommand;
        public ICommand TrailerSelectorCommand
        {
            get
            {
                return (_trailerSelectorCommand = _trailerSelectorCommand ?? new MvxCommand<Vehicle>(v => TrailerDetail(v)));
            }
        }

        private MvxCommand<Vehicle> _notrailerSelectorCommand;
        public ICommand NoTrailerSelectorCommand
        {
            get
            {
                return (_notrailerSelectorCommand = _notrailerSelectorCommand ?? new MvxCommand<Vehicle>(v => TrailerDetail(null)));
            }
        }

        public void TrailerDetail(Vehicle v)
        {
            if (v == null)
            {
                //This will take to the next view model with a trailer value of null.
                Mvx.Resolve<IUserInteraction>().Alert("No Trailer is Selected!");
            }
            else
            {
                //This will take to the next view model with a trailer value of null.
                Mvx.Resolve<IUserInteraction>().Alert("Trailer Selected is " + v.Title);
            }
        }
    }
}
