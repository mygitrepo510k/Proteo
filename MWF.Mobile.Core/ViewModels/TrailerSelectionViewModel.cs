﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
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

        private Trailer _trailer;

        public class Nav
        {
            public Guid ID { get; set; }
        }

        public void Init(Nav nav)
        {
            Trailer = new Trailer
            {
                ID = nav.ID
                
            };
        }

        public Trailer Trailer
        {
            get { return _trailer; }
            set { _trailer = value; RaisePropertyChanged(() => Trailer); }
        }

        private IEnumerable<Trailer> _trailerList;
        public TrailerSelectionViewModel(ITrailerRepository trailerRepository)
        {
            Trailers = _trailerList = trailerRepository.GetAll();
        }

        private IEnumerable<Trailer> _trailers;
        public IEnumerable<Trailer> Trailers
        {
            get { return _trailers; }
            set { _trailers = value; RaisePropertyChanged(() => Trailers); }
        }

        private MvxCommand<Trailer> _trailerSelectorCommand;
        public ICommand TrailerSelectorCommand
        {
            get
            {
                return (_trailerSelectorCommand = _trailerSelectorCommand ?? new MvxCommand<Trailer>(v => TrailerDetail(v)));
            }
        }

        private MvxCommand<Trailer> _notrailerSelectorCommand;
        public ICommand NoTrailerSelectorCommand
        {
            get
            {
                return (_notrailerSelectorCommand = _notrailerSelectorCommand ?? new MvxCommand<Trailer>(v => TrailerDetail(null)));
            }
        }

        public void TrailerDetail(Trailer v)
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
