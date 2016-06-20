using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
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
    public class DriverSignatureViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;

        private string _driverName;
        private string _driverSignature;        

        public DriverSignatureViewModel(ICloseApplication closeApplication,
            INavigationService navigationService, IRepositories repositories)
        {
            _closeApplication = closeApplication;
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public string DriverName
        {
            get { return _driverName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Mvx.Resolve<ICustomUserInteraction>().AlertAsync("To complete, please enter your name");
                else _driverName = value;
            }
        }

        public string DriverSignature
        {
            get { return _driverSignature; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Mvx.Resolve<ICustomUserInteraction>().AlertAsync("To continue, please sign on the pad");
                else _driverSignature = value;
            }
        }

        public override string FragmentTitle
        {
            get { return "Sign Device Out"; }
        }

        public string NameText
        {
            get { return "Name"; }
        }

        public string CompleteButtonLabel
        {
            get { return "Complete"; }
        }

        public async Task<bool> OnBackButtonPressedAsync()
        {
            var closeApp = true;

#if DEBUG
            closeApp = !await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("DEBUGGING: Return to Customer Code screen?", cancelButton: "No, close the app");
#endif

            if (closeApp)
                _closeApplication.CloseApp();
            else
                ShowViewModel<TermsAndConditionsViewModel>();

            return false;
        }

        private MvxCommand _completeCommand;
        public System.Windows.Input.ICommand CompleteCommand
        {
            get { return (_completeCommand = _completeCommand ?? new MvxCommand(async () => await this.MoveToNextAsync())); }
        }

        private MvxCommand _sendDiagnosticsCommand;
        public System.Windows.Input.ICommand SendDiagnosticsCommand
        {
            get { return (_sendDiagnosticsCommand = _sendDiagnosticsCommand ?? new MvxCommand(async () => await this.SendDiagnosticsAsync())); }
        }

        public async Task MoveToNextAsync()
        {
            if (string.IsNullOrEmpty(DriverName))
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("To complete, please enter your name");

            if (string.IsNullOrEmpty(DriverSignature))
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("To continue, please sign on the pad");

            if (!string.IsNullOrEmpty(DriverName) && !string.IsNullOrEmpty(DriverSignature))
            {
                NavData<Models.CheckInOutData> navData = _navigationService.CurrentNavData as NavData<Models.CheckInOutData>;
                navData.Data.actualActionPerformed = 2;
                navData.Data.actualIMEI = Mvx.Resolve<IDeviceInfo>().IMEI;
                navData.Data.signature = DriverSignature;
                navData.Data.driverName = DriverName;

                var appProfile = await _repositories.ApplicationRepository.GetAsync();
                string deviceEventUrl = appProfile.DeviceEventURL;

                //If you change pwd1stHalf below remember to change in Proteo Analytics too 
                string pwd1stHalf = "{6A50F099-DEA4-4B34-9D2C-73C438D8A005}";

                HttpService service = new HttpService();
                HttpResult result = await service.PostJsonWithAuthAsync(JsonConvert.SerializeObject(navData.Data), 
                    deviceEventUrl, "ProteoMobile", pwd1stHalf + Guid.NewGuid().ToString());
                if (result.Succeeded)
                {
                    appProfile.DeviceCheckInRequired = true;
                    await _navigationService.MoveToNextAsync();
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync(
                        "The QR code data and device details did not match. Please ensure that you are checking out the correct device.");
                }
                else
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync(
                        "Unable to communicate with Device management. Please ensure mobile data is ON.");
                }
            }
        }

        public Task SendDiagnosticsAsync()
        {
            NavData<object> navData = new NavData<object>();
            navData.OtherData["Diagnostics"] = true;
            return _navigationService.MoveToNextAsync(navData);
        }
    }
}
