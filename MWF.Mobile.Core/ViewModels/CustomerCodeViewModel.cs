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
        IReachability _reachability;
        public CustomerCodeViewModel()
        {
            _reachability = Mvx.Resolve<IReachability>();
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
                _enterCodeCommand = _enterCodeCommand ?? new MvxCommand(async ()=> await EnterCodeAsync());
                return _enterCodeCommand;
            }
        }

        private async Task EnterCodeAsync()
        {

            if(!_reachability.IsConnected())
            {
                await Mvx.Resolve<IUserInteraction>().AlertAsync("An Internet connection is required");
            }
            else
            {
                IsBusy = true;
            }

            
            //TODO fire this off to BlueSphere

            //TODO if success then save code to database
        }


    }
}
