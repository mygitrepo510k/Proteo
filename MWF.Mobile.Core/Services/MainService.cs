using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
