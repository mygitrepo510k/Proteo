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
    public class SafetyCheckFaultViewModel : BaseModalViewModel<bool>
    {

        #region Private Members

        private MvxCommand _doneCommand;
        private IStartupInfoService _startupInfoService;
        private SafetyCheckFault _safetyCheckFault;             // working copy of safety check fault for duration of this screen
        private SafetyCheckFault _originalSafetyCheckFault;     // original copy of safety check fault we'll write to when "done" is clicked
        private string _faultTypeText;

        #endregion


        #region Construction

        public SafetyCheckFaultViewModel(IStartupInfoService startupInfoService)
        {
            _startupInfoService = startupInfoService;
        }


        public void Init(SafetyCheckNavItem item)
        {
            base.Init(item.MessageID);

            // Get the safety check fault to display
            if (item.IsVehicle)
            {
                _originalSafetyCheckFault = _startupInfoService.CurrentVehicleSafetyCheckData.Faults.SingleOrDefault(f => f.ID == item.FaultID);
            }
            else
            {
                _originalSafetyCheckFault = _startupInfoService.CurrentTrailerSafetyCheckData.Faults.SingleOrDefault(f => f.ID == item.FaultID);
            }

            _faultTypeText = item.FaultTypeText;

            _safetyCheckFault = _originalSafetyCheckFault.Clone();
            
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

        public string CommentHintText
        {
            get { return "Type Comment"; }
        }

        public string CommentText
        {
            get { return _safetyCheckFault.Comment; }
            set 
            {
                _safetyCheckFault.Comment = value;
                RaisePropertyChanged(() => CommentText);
                RaisePropertyChanged(() => HasCommentText);
            }
        }

        public string InstructionsText
        {
            get { return "Add a comment and add an image"; }
        }

        public bool HasCommentText
        {
            get { return !string.IsNullOrEmpty(_safetyCheckFault.Comment); }
        }

        public string DiscretionaryOrFailureText
        {
            get { return _faultTypeText; }
        }

        public System.Windows.Input.ICommand DoneCommand
        {
            get { return (_doneCommand = _doneCommand ?? new MvxCommand( () => DoDoneCommand())); }
        }

        #endregion

        #region Private Methods

        private void DoDoneCommand()
        {
            _originalSafetyCheckFault.ValuesFrom(_safetyCheckFault);
            ReturnResult(true);
        }

        #endregion


    }


}
