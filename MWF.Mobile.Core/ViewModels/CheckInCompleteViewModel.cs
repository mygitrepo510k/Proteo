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
    public class CheckInCompleteViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly ICloseApplication _closeApplication;
        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;

        private string _status;

        public CheckInCompleteViewModel(ICloseApplication closeApplication,
            INavigationService navigationService, IRepositories repositories)
        {
            _closeApplication = closeApplication;
            _navigationService = navigationService;
            _repositories = repositories;

            Status = "Check In in progress. The application will automatically redirect to Check Out page when the process completes successfully.";
        }

        public override string FragmentTitle
        {
            get { return "Check In Status"; }
        }

        public string CheckInStatusLabel
        {
            get { return "Check In Status"; }
        }

        public string Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                RaisePropertyChanged(() => Status);
            }
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
                ShowViewModel<CustomerCodeViewModel>();

            return false;
        }

        public async Task CheckInDeviceAsync()
        {
            NavData<Models.CheckInOutData> navData = _navigationService.CurrentNavData as NavData<Models.CheckInOutData>;
            navData.Data.actualActionPerformed = 1;
            navData.Data.actualIMEI = Mvx.Resolve<IDeviceInfo>().IMEI;
            navData.Data.signature = string.Empty;
            navData.Data.driverName = string.Empty;

            var appProfile = await _repositories.ApplicationRepository.GetAsync();
#if DEBUG
            string deviceEventUrl = "http://10.0.2.2:61001/api/devicemanagement/recordevent";
#else
            string deviceEventUrl = appProfile.DeviceEventURL;
#endif
            //If you change pwd1stHalf below remember to change in Proteo Analytics too 
            string pwd1stHalf = "{6A50F099-DEA4-4B34-9D2C-73C438D8A005}";

            HttpService service = new HttpService();
            HttpResult result = await service.PostJsonWithAuthAsync(JsonConvert.SerializeObject(navData.Data),
                deviceEventUrl, "ProteoMobile", pwd1stHalf + Guid.NewGuid().ToString());
            if (result.Succeeded)
            {
                Status = "Device successfully checked in.";
                appProfile.DeviceCheckInRequired = false;
                Task.Delay(2000).ContinueWith((x) => ShowViewModel<CheckOutViewModel>());
            }
            else if (result.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
            {
                Status = "The QR code data and device details did not match. Please ensure that you are checking in the correct device.";
            }
            else
            {
                Status = "Unable to communicate with Device management. Please ensure mobile data is ON.";
            }
        }        
    }
}
