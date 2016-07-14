﻿using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class CheckInViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;

        private string _message;

        public CheckInViewModel(ICloseApplication closeApplication,
            INavigationService navigationService)
        {
            _closeApplication = closeApplication;
            _navigationService = navigationService;
        }

        public override string FragmentTitle
        {
            get { return "Check In Device"; }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged(() => Message);
            }
        }

        public string ContinueButtonLabel
        {
            get { return "Continue"; }
        }

        public string ScanAgainButtonLabel
        {
            get { return "Scan Again"; }
        }

        public string ScannedQRCode { get; set; }

        public async Task<bool> OnBackButtonPressedAsync()
        {
            await Task.Run(() => ShowViewModel<PasscodeViewModel>());
            return false;
        }

        private MvxCommand _continueCommand;
        public System.Windows.Input.ICommand ContinueCommand
        {
            get { return (_continueCommand = _continueCommand ?? new MvxCommand(async () => await this.MoveToNextAsync())); }
        }        

        private MvxCommand _sendDiagnosticsCommand;
        public System.Windows.Input.ICommand SendDiagnosticsCommand
        {
            get { return (_sendDiagnosticsCommand = _sendDiagnosticsCommand ?? new MvxCommand(async () => await this.SendDiagnosticsAsync())); }
        }

        public Task MoveToNextAsync()
        {
            if (string.IsNullOrEmpty(ScannedQRCode))
                return Mvx.Resolve<ICustomUserInteraction>().AlertAsync(Message);

            NavData<Models.CheckInOutData> navData = new NavData<Models.CheckInOutData>();
            navData.Data = new Models.CheckInOutData();
            navData.Data.qrData = JsonConvert.DeserializeObject<Models.QRData>(this.ScannedQRCode);
            return _navigationService.MoveToNextAsync(navData);
        }

        public Task SendDiagnosticsAsync()
        {
            NavData<object> navData = new NavData<object>();
            navData.OtherData["Diagnostics"] = true;
            return _navigationService.MoveToNextAsync(navData);
        }
    }
}
