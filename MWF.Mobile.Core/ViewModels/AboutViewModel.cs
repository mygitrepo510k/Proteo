using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{
    public class AboutViewModel : MvxViewModel
    {
        private string _imei;
        public string IMEI
        {
            get { return _imei; }
            set { _imei = value; RaisePropertyChanged(() => IMEI); }
        }

        private string _androidId;
        public string AndroidId
        {
            get { return _androidId; }
            set { _androidId = value; RaisePropertyChanged(() => AndroidId); }
        }

        private string _serialNumber;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; RaisePropertyChanged(() => SerialNumber); }
        }

        private string _OsVersion;
        public string OsVersion
        {
            get { return _OsVersion; }
            set { _OsVersion = value; RaisePropertyChanged(() => OsVersion); }
        }

        private string _model;
        public string Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(() => Model); }
        }

        private string _manufacturer;
        public string Manufacturer
        {
            get { return _manufacturer; }
            set { _manufacturer = value; RaisePropertyChanged(() => Manufacturer); }
        }
        
                
        public AboutViewModel(IDeviceInfo deviceIdentifier)
        {
            IMEI = deviceIdentifier.IMEI;
            AndroidId = deviceIdentifier.AndroidId;
            SerialNumber = deviceIdentifier.SerialNumber;
            OsVersion = deviceIdentifier.OsVersion;
            Model = deviceIdentifier.Model;
            Manufacturer = deviceIdentifier.Manufacturer;
        }
    }
}
