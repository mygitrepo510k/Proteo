using Cirrious.CrossCore;
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
    public class QRCodeViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;

        public QRCodeViewModel(ICloseApplication closeApplication,
            INavigationService navigationService)
        {
            _closeApplication = closeApplication;
            _navigationService = navigationService;
        }

        public override string FragmentTitle
        {
            get { return "Scan QR Code"; }
        }

        public string ContinueButtonLabel
        {
            get { return "Continue"; }
        }

        public string ScannedQRCode { get; set; }

        public async Task<bool> OnBackButtonPressedAsync()
        {
            var closeApp = true;

#if DEBUG
            closeApp = !await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("DEBUGGING: Return to Customer Code screen?", cancelButton: "No, close the app");
#endif

            if (closeApp)
                _closeApplication.CloseApp();
            else
                ShowViewModel<CustomerCodeViewModel>();

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
