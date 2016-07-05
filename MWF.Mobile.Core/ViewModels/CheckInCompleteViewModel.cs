using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Enums;
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
            await Task.Run(() => ShowViewModel<CheckInViewModel>());
            return false;
        }

        public async Task CheckInDeviceAsync()
        {
            NavData<Models.CheckInOutData> navData = _navigationService.CurrentNavData as NavData<Models.CheckInOutData>;
            navData.Data.actualActionPerformed = CheckInOutActions.CheckIn;
            navData.Data.actualIMEI = Mvx.Resolve<IDeviceInfo>().IMEI;
            navData.Data.signature = string.Empty;
            navData.Data.driverName = string.Empty;

            CheckInOutService service = new CheckInOutService(_repositories);
            HttpResult result = await service.CheckInDevice(navData.Data);
            if (result.Succeeded)
            {
                Status = "Device successfully checked in.";
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
