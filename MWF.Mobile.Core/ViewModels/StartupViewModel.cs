using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: BaseActivityViewModel
    {
        private readonly IRepositories _repositories = null; 
        
        public StartupViewModel(IRepositories repositories)
        {
            _repositories = repositories;
        }

        public override async void Start()
        {
            base.Start();
            await this.SetInitialViewModelAsync();
        }

        private async Task SetInitialViewModelAsync()
        {
            var customerRepository = _repositories.CustomerRepository;
            var customerRepositoryData = await customerRepository.GetAllAsync();

            if (customerRepositoryData.Any())
                this.SetInitialViewModel<PasscodeViewModel>();
            else
                this.SetInitialViewModel<CustomerCodeViewModel>();
        }

    }

}
