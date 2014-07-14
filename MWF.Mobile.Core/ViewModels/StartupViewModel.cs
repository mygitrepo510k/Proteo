using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {

        public StartupViewModel(Services.IAuthenticationService authenticationService, Services.IGatewayService gatewayService, Portable.IReachability reachableService, Services.IDataService dataService)
        {
           //this.InitialViewModel = new PasscodeViewModel(authenticationService);
            //this.InitialViewModel = new PasscodeViewModel(authenticationService);
           //this.InitialViewModel = new CustomerCodeViewModel();
           //this.InitialViewModel = new VehicleListViewModel(new Services.VehicleExtractService());
            // this.InitialViewModel = new PasscodeViewModel(authenticationService);
            this.InitialViewModel = new CustomerCodeViewModel(gatewayService, reachableService, dataService);
            //this.InitialViewModel = new VehicleListViewModel(new Services.VehicleExtractService());
        }
    }
}
