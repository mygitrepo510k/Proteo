using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Portable;
using Chance.MvvmCross.Plugins.UserInteraction;



namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {

        public StartupViewModel(IAuthenticationService authenticationService, IGatewayService gatewayService, Portable.IReachability reachableService, IDataService dataService, IRepositories repositories, IDeviceInfo deviceInfo, IUserInteraction userInteraction)
        {
//#if DEBUG
//            Mvx.Resolve<IUserInteraction>().Confirm("DEBUGGING: clear all device setup data from the local database?", () => DEBUGGING_ClearAllData(repositories));
//#endif

            var customerRepository = repositories.CustomerRepository;

            if (customerRepository.GetAll().Any())
            {
                this.InitialViewModel = new PasscodeViewModel(authenticationService);
            }
            else
            {
                this.InitialViewModel = new CustomerCodeViewModel(gatewayService, reachableService, dataService, repositories, userInteraction);
            }
        }

        private void DEBUGGING_ClearAllData(IRepositories repositories)
        {
            ClearAllDataFromTable(repositories.ApplicationRepository);
            ClearAllDataFromTable(repositories.CustomerRepository);
            ClearAllDataFromTable(repositories.DeviceRepository);
            ClearAllDataFromTable(repositories.DriverRepository);
            ClearAllDataFromTable(repositories.SafetyProfileRepository);
            ClearAllDataFromTable(repositories.TrailerRepository);
            ClearAllDataFromTable(repositories.VehicleRepository);
            ClearAllDataFromTable(repositories.VerbProfileRepository);
        }

        private void ClearAllDataFromTable<T>(Repositories.IRepository<T> repository) where T : IBlueSphereEntity, new()
        {
            var rows = repository.GetAll().ToList();
            
            foreach (var row in rows)
            {
                repository.Delete(row);
            }
        }


    }
}
