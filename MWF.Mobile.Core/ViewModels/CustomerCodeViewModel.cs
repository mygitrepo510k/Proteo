using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Portable;
using Chance.MvvmCross.Plugins.UserInteraction;

namespace MWF.Mobile.Core.ViewModels
{

    public class CustomerCodeViewModel : MvxViewModel
    {

        private readonly Services.IGatewayService _gatewayService;
        private readonly IReachability _reachability;

        public CustomerCodeViewModel(Services.IGatewayService gatewayService, IReachability reachability)
        {
            _gatewayService = gatewayService;
            _reachability = reachability;
        }

        private string _customerCode = null;
        public string CustomerCode
        {
            get { return _customerCode; }
            set { _customerCode = value; RaisePropertyChanged(() => CustomerCode); }
        }

        public string EnterButtonLabel
        {
            get { return "Save Customer Code"; }
        }

        public string CustomerCodeLabel
        {
            get { return "Please enter your Customer Code"; }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        private MvxCommand _enterCodeCommand;
        public System.Windows.Input.ICommand EnterCodeCommand
        {
            get
            {
                _enterCodeCommand = _enterCodeCommand ?? new MvxCommand(async () => await EnterCodeAsync());
                return _enterCodeCommand;
            }
        }

        private async Task EnterCodeAsync()
        {
            if (!_reachability.IsConnected())
            {
                await Mvx.Resolve<IUserInteraction>().AlertAsync("An Internet connection is required");
            }
            else
            {
                this.IsBusy = true;
                
                if (await this.AssociateDeviceToCustomer() && await this.SetupDevice())
                {
                    //TODO: if success then save code to database
                }

                this.IsBusy = false;
            }
        }

        private async Task<bool> AssociateDeviceToCustomer()
        {
            //TODO: implement this
            return true;
        }

        private async Task<bool> SetupDevice()
        {
            // TODO: Get verb profile titles from config or somewhere?
            var verbProfileTitles = new[] { "Palletforce", "Cancel", "Complete", "Suspend" };

            var device = await _gatewayService.GetDevice();
            var verbProfiles = verbProfileTitles.Select(async vpt => await _gatewayService.GetVerbProfile(vpt)).ToList();
            var applicationProfile = await _gatewayService.GetApplicationProfile();
            var drivers = await _gatewayService.GetDrivers();
            var vehicleViews = await _gatewayService.GetVehicleViews();
            var vehicles = vehicleViews.Select(vv => vv.Title).ToDictionary(vvt => vvt, async vvt => await _gatewayService.GetVehicles(vvt));
            var safetyProfiles = await _gatewayService.GetSafetyProfiles();

            //TODO: write all this retrieved data to the database

            //TODO: call fwRegisterDevice - what does this actually do?

            return true;
        }

    }
}
