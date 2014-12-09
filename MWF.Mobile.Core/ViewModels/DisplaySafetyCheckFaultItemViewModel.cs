using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class DisplaySafetyCheckFaultItemViewModel
        : MvxViewModel
    {

        private string _faultType;
        public string FaultType
        {
            get { return _faultType; }
            set { _faultType = value; RaisePropertyChanged(() => FaultType); }
        }

        private string _faultCheckTitleAndComment;
        public string FaultCheckTitleAndComment
        {
            get { return _faultCheckTitleAndComment; }
            set { _faultCheckTitleAndComment = value; RaisePropertyChanged(() => FaultCheckTitleAndComment); }
        }

        private string _faultStatus;
        public string FaultStatus
        {
            get { return _faultStatus; }
            set { _faultStatus = value; RaisePropertyChanged(() => FaultStatus); }
        }

    }
}
