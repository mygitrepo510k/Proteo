using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private SafetyCheckEnum _checkStatus;
        public SafetyCheckEnum CheckStatus
        {
            get { return _checkStatus; }
            set 
            { 
                if (_checkStatus == SafetyCheckEnum.DiscretionaryPass ||
                    _checkStatus == SafetyCheckEnum.Failed)
                {
                    Mvx.Resolve<IUserInteraction>().Confirm(("Are you sure you wish to change this safety check to passed?"), isConfirmed =>
                    {
                        if (isConfirmed)
                        {
                            _checkStatus = value;
                            _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                            RaisePropertyChanged(() => CheckStatus);
                        }
                    }, "Change Check Status");
                }
                else
                {
                    _checkStatus = value;
                    _safetyCheckViewModel.CheckSafetyCheckItemsStatus();
                    RaisePropertyChanged(() => CheckStatus);
                }
            }
        }
        
        private SafetyCheckViewModel _safetyCheckViewModel;

        public SafetyCheckItemViewModel(SafetyCheckViewModel safetyCheckViewModel)
        {
            _safetyCheckViewModel = safetyCheckViewModel;
            _checkStatus = SafetyCheckEnum.NotSet;
        }
    }
}
