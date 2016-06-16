using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {
        private readonly IRepositories _repositories = null; 
        
        public StartupViewModel(IMvxViewModelLoader viewModelLoader, IRepositories repositories)
            : base(viewModelLoader)
        {
            _repositories = repositories;
        }

        public async Task Init()
        {
            await this.SetInitialViewModelAsync();
        }

        private async Task SetInitialViewModelAsync()
        {
            var customerRepository = _repositories.CustomerRepository;
            var customerRepositoryData = await customerRepository.GetAllAsync();
            var appProfile = await _repositories.ApplicationRepository.GetAsync();

            if (customerRepositoryData.Any())
            {
                if (appProfile.DeviceCheckOutRequired)
                    this.SetInitialViewModel<CheckOutViewModel>();
                else this.SetInitialViewModel<PasscodeViewModel>();
            }
            else
                this.SetInitialViewModel<CustomerCodeViewModel>();
        }

    }

}
