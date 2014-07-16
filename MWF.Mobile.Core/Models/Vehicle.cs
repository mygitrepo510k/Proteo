﻿using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class Vehicle : BaseVehicle
    {

        public Vehicle()
            : base()
        { }

        public Vehicle(BaseVehicle baseVehicle)
            : base()
        {
            this.ID = baseVehicle.ID;
            this.Title = baseVehicle.Title;
            this.Registration = baseVehicle.Registration;
            this.SafetyCheckProfileIntLink = baseVehicle.SafetyCheckProfileIntLink;
            this.IsTrailer = IsTrailer;
        }
    }
}
