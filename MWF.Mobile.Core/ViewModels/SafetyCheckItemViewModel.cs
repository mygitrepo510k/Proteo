using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckItemViewModel : MvxViewModel
    {

        private INavigationService _navigationService;

        public SafetyCheckItemViewModel(SafetyCheckViewModel safetyCheckViewModel, INavigationService navigationService)
        {
            _safetyCheckViewModel = safetyCheckViewModel;
            _checkStatus = Enums.SafetyCheckStatus.NotSet;
            _navigationService = navigationService;
        }

        private SafetyCheckViewModel _safetyCheckViewModel;

        private Guid _id;
        public Guid ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsVehicle { get; set; }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private bool _isDiscretionaryQuestion;
        public bool IsDiscretionaryQuestion
        {
            get { return _isDiscretionaryQuestion; }
            set { _isDiscretionaryQuestion = value; }
        }

        // The safety check fault model that will be persisted back to bluesphere/local db
        public SafetyCheckFault SafetyCheckFault { get; set; }

        private Enums.SafetyCheckStatus _checkStatus;
        public Enums.SafetyCheckStatus CheckStatus
        {
            get { return _checkStatus; }
            set 
            { 
                if ((_checkStatus == Enums.SafetyCheckStatus.DiscretionaryPass ||
                    _checkStatus == Enums.SafetyCheckStatus.Failed) && value == Enums.SafetyCheckStatus.Passed)
                {
                    Mvx.Resolve<IUserInteraction>().Confirm(("Change '"+ Title +"' to passed?"), isConfirmed =>
                    {
                        if (isConfirmed)
                        {
                            _checkStatus = value;
                            _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                            this.SafetyCheckFault.Comment = string.Empty;
                            this.SafetyCheckFault.Images = new List<Image>();
                            this.SafetyCheckFault.Status = value;
                            this.SafetyCheckFault.IsDiscretionaryPass = false;
                            RaisePropertyChanged(() => CheckStatus);
                        }
                    }, "Change Status", "Change");
                }
                else
                {

                    if (value == Enums.SafetyCheckStatus.DiscretionaryPass || value == Enums.SafetyCheckStatus.Failed)
                    {
                        string faultTypeText = (value == Enums.SafetyCheckStatus.DiscretionaryPass) ? "Discretionary Pass" : "Failure";


                        //var navItem = new SafetyCheckNavItem() { FaultID = this.SafetyCheckFault.ID, IsVehicle = this.IsVehicle, FaultTypeText = faultTypeText };

                        NavData<SafetyCheckFault> safetyCheckNavItem = new NavData<SafetyCheckFault>() { Data = this.SafetyCheckFault };
                        safetyCheckNavItem.OtherData["FaultTypeText"] = faultTypeText;

                        _navigationService.ShowModalViewModel<SafetyCheckFaultViewModel, bool>(_safetyCheckViewModel, safetyCheckNavItem, (faultLogged) =>
                        {
                            if (faultLogged)
                            {
                                _checkStatus = value;
                                _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                                this.SafetyCheckFault.Status = value;
                                this.SafetyCheckFault.IsDiscretionaryPass = _checkStatus == Enums.SafetyCheckStatus.DiscretionaryPass;
                                RaisePropertyChanged(() => CheckStatus);
                            }

                        });                      

                    }
                    else
                    {
                        _checkStatus = value;
                        _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                        this.SafetyCheckFault.Status = value;
                        this.SafetyCheckFault.IsDiscretionaryPass = _checkStatus == Enums.SafetyCheckStatus.DiscretionaryPass;
                        RaisePropertyChanged(() => CheckStatus);
                    }
                   
               
                }
            }
        }

    }
}
