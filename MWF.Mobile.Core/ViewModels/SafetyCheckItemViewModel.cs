using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.ViewModels
{
    public enum SafetyCheckEnum
    {
        NotSet,
        Passed, 
        DiscretionaryPass, 
        Failed
    }

    public class SafetyCheckItemViewModel : MvxViewModel
    {
        public SafetyCheckItemViewModel(SafetyCheckViewModel safetyCheckViewModel)
        {
            _safetyCheckViewModel = safetyCheckViewModel;
            _checkStatus = SafetyCheckEnum.NotSet;
        }

        private SafetyCheckViewModel _safetyCheckViewModel;

        private Guid _id;
        public Guid ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private bool _isDiscretionaryQuestion;
        public bool IsDiscreationaryQuestion
        {
            get { return _isDiscretionaryQuestion; }
            set { _isDiscretionaryQuestion = value; }
        }

        // The safety check fault model that will be persisted back to bluesphere/local db
        public SafetyCheckFault SafetyCheckFault { get; set; }


        private SafetyCheckEnum _checkStatus;
        public SafetyCheckEnum CheckStatus
        {
            get { return _checkStatus; }
            set 
            { 
                if ((_checkStatus == SafetyCheckEnum.DiscretionaryPass ||
                    _checkStatus == SafetyCheckEnum.Failed) && value == SafetyCheckEnum.Passed)
                {
                    Mvx.Resolve<IUserInteraction>().Confirm(("Change this item to passed?"), isConfirmed =>
                    {
                        if (isConfirmed)
                        {
                            _checkStatus = value;
                            _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                            this.SafetyCheckFault.IsDiscretionaryPass = false;
                            RaisePropertyChanged(() => CheckStatus);
                        }
                    }, "Change Status");
                }
                else
                {

                    if (value == SafetyCheckEnum.DiscretionaryPass || value == SafetyCheckEnum.Failed)
                    {
                        string faultTypeText = (value == SafetyCheckEnum.DiscretionaryPass) ? "Discretionary Pass" : "Failure";
                        var navItem = new SafetyCheckNavItem() { FaultID = this.SafetyCheckFault.ID, IsVehicle = this.SafetyCheckFault.Title.StartsWith("VEH"), FaultTypeText = faultTypeText };
                        _safetyCheckViewModel.ShowModalViewModel<SafetyCheckFaultViewModel, bool>(navItem, (faultLogged) =>
                        {
                            if (faultLogged)
                            {
                                _checkStatus = value;
                                _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                                this.SafetyCheckFault.IsDiscretionaryPass = _checkStatus == SafetyCheckEnum.DiscretionaryPass;
                                RaisePropertyChanged(() => CheckStatus);
                            }

                        });

                    }
                    else
                    {
                        _checkStatus = value;
                        _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                        this.SafetyCheckFault.IsDiscretionaryPass = _checkStatus == SafetyCheckEnum.DiscretionaryPass;
                        RaisePropertyChanged(() => CheckStatus);
                    }
                   
               
                }
            }
        }

    }
}
