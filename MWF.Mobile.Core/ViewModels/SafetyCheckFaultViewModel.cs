using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckFaultViewModel : BaseFragmentViewModel
    {

        #region Private Members

        private MvxCommand _doneCheckCommand;
        private IStartupInfoService _startupInfoService;
        private SafetyCheckFault _safetyCheckFault;

        #endregion


        #region Construction

        public SafetyCheckFaultViewModel(IStartupInfoService startupInfoService)
        {
            _startupInfoService = startupInfoService;
        }


        public void Init(SafetyCheckFaultNavItem item)
        {
            // Get the safety check fault
            _safetyCheckFault = _startupInfoService.CurrentSafetyCheckData.Faults.Single(f => f.ID == item.ID);
        }




        #endregion


        #region Public Properties

        public override string FragmentTitle
        {
            get { return "Falut Screen"; }
        }

        public string DoneButtonLabel
        {
            get { return "Done"; }
        }

        public string CheckTypeText
        {
            get { return _safetyCheckFault.Title;  }
        }

        public string DiscretionaryOrFailureText
        {
            get { return (_safetyCheckFault.IsDiscretionaryPass) ? "Discretionary Pass" : "Failure"; }
        }

        public System.Windows.Input.ICommand DoneCheckCommand
        {
            get { return (_doneCheckCommand = _doneCheckCommand ?? new MvxCommand(async () => await Mvx.Resolve<IUserInteraction>().AlertAsync("Fault Logged."))); }
        }

        #endregion


        public class SafetyCheckFaultNavItem
        {
            public Guid ID { get; set; }
        }




    }
}
