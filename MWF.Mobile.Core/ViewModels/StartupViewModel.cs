using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {

        public StartupViewModel(Services.IAuthenticationService authenticationService)
        {
           // this.InitialViewModel = new PasscodeViewModel(authenticationService);
           // this.InitialViewModel = new CustomerCodeViewModel();
            Services.VehicleExtractService vehicleService = new Services.VehicleExtractService();
            this.InitialViewModel = new AllVehicleDisplayViewModel(vehicleService);
        }

    }

}
