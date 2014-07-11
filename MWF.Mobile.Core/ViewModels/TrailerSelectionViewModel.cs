using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
