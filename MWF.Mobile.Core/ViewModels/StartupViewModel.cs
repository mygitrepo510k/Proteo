using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Portable;


namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {

        public StartupViewModel(IAuthenticationService authenticationService, IGatewayService gatewayService, Portable.IReachability reachableService, IDataService dataService, IRepositories repositories, IDeviceInfo deviceInfo)          
        {

            var customerRepository = repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
            {
                this.InitialViewModel = new PasscodeViewModel(authenticationService);
            }
            else
            {
                this.InitialViewModel = new CustomerCodeViewModel(gatewayService, reachableService, dataService, repositories);
            }
        }
    }
}
