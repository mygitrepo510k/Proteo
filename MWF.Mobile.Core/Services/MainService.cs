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


        #region Public Members

        public Driver CurrentDriver { get; set; }
        public Vehicle CurrentVehicle { get; set; }

        public MobileData CurrentMobileData { get; set; }
        public MobileApplicationDataChunkContentActivity CurrentDataChunkActivity { get; set; }

        #endregion Public Members

    }
}
