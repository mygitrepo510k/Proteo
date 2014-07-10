using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWF.Mobile.Core.ViewModels
{

    public class VehicleDetailViewModel
        :MvxViewModel
    {

        private Vehicle _item;

        public class Nav
        {
            public int ID { get; set; }
        }

        public void Init(Nav navigation)
        {
            Item = new Vehicle
            {
                ID = 1,
                Registration = "EG11 ULT",
                Title = "test truck"
            };
        }

        public Vehicle Item
        {
            get { return _item; }
            set { _item = value; RaisePropertyChanged(() => Item); }
        }
    }
}
