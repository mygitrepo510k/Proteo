using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public class MainService
        : MvxNavigatingObject, IMainService
    {

        private IStartupService _startUpService;

        public MainService(IStartupService startupService)
        {
            _startUpService = startupService;
        }

        #region Public Members

        public Driver CurrentDriver 
        { 
            get
            {
                return _startUpService.LoggedInDriver;
            }

        }

        public Vehicle CurrentVehicle 
        { 
            get
            {
                return _startUpService.CurrentVehicle;
            }
        }

        public Models.Trailer CurrentTrailer 
        { 
            get
            {
                return _startUpService.CurrentTrailer;
            }
            set
            {
                _startUpService.CurrentTrailer = value;
            }
        }

        #endregion Public Members

    }
}
