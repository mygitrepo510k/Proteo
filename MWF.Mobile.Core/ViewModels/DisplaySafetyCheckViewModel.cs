using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class DisplaySafetyCheckViewModel
        : BaseInstructionNotificationViewModel, IBackButtonHandler
    {
        private IMainService _mainService;
        private INavigationService _navigationService;
        private Repositories.IRepositories _repositories;
        private LatestSafetyCheck _latestSafetyCheckData;

        public DisplaySafetyCheckViewModel(IMainService mainService, INavigationService navigationService, Repositories.IRepositories repositories)
        {

            _mainService = mainService;
            _navigationService = navigationService;
            _repositories = repositories;
            SafetyCheckFaultItemViewModels = new List<DisplaySafetyCheckFaultItemViewModel>();

            _latestSafetyCheckData = _repositories.LatestSafetyCheckRepository.GetForDriver(_mainService.CurrentDriver.ID);

            //If there is no safety check data to view, then sends them back to where they came.
            if (_latestSafetyCheckData.VehicleSafetyCheck == null && _latestSafetyCheckData.TrailerSafetyCheck == null)
            {
                NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = (_mainService.OnManifestPage) ? Guid.Empty : _mainService.CurrentMobileData.ID };

                Mvx.Resolve<IUserInteraction>().Alert("A safety check profile for your vehicle and/or trailer has not been completed - Refer to your manual safety check.",
                    () => { _navigationService.MoveToNext(navItem); });
            }

            if (_latestSafetyCheckData.VehicleSafetyCheck != null)
                GenerateSafetyCheckFaultItems(_latestSafetyCheckData.VehicleSafetyCheck.Faults, false);

            if (_latestSafetyCheckData.TrailerSafetyCheck != null)
                GenerateSafetyCheckFaultItems(_latestSafetyCheckData.TrailerSafetyCheck.Faults, true);

        }

        #region Public Properties

        public string VehicleRegistration
        {
            get { return "Vehicle: " + ((_latestSafetyCheckData.VehicleSafetyCheck != null) ? _latestSafetyCheckData.VehicleSafetyCheck.VehicleRegistration : ""); }
        }

        public string TrailerRegistration
        {
            get { return "Trailer: " + ((_latestSafetyCheckData.TrailerSafetyCheck != null) ? _latestSafetyCheckData.TrailerSafetyCheck.VehicleRegistration : ""); }
        }

        public string VehicleSafetyCheckStatus
        {
            get { return "Checked: " + ((_latestSafetyCheckData.VehicleSafetyCheck != null) ? _latestSafetyCheckData.VehicleSafetyCheck.EffectiveDate.ToString("g") : ""); }
        }

        public string TrailerSafetyCheckStatus
        {
            get { return "Checked: " + ((_latestSafetyCheckData.TrailerSafetyCheck != null) ? _latestSafetyCheckData.TrailerSafetyCheck.EffectiveDate.ToString("g") : ""); }
        }

        public string SafetyCheckStatusKey
        {
            get { return "* FP = Full Pass; DP = Discretionary Pass; F = Fail"; }
        }

        public string SafetyCheckTypeColumnHeader
        {
            get { return "Type"; }
        }

        public string SafetyCheckCheckTitleColumnHeader
        {
            get { return "Check - Comment"; }
        }

        public string SafetyCheckStatusColumnHeader
        {
            get { return "Status"; }
        }

        public string DriverName
        {
            get { return _mainService.CurrentDriver.DisplayName; }
        }

        private List<DisplaySafetyCheckFaultItemViewModel> _safetyCheckFaultItemViewModels;
        public List<DisplaySafetyCheckFaultItemViewModel> SafetyCheckFaultItemViewModels
        {
            get { return _safetyCheckFaultItemViewModels; }
            set { _safetyCheckFaultItemViewModels = value; RaisePropertyChanged(() => SafetyCheckFaultItemViewModels); }
        }

        private MvxCommand<DisplaySafetyCheckFaultItemViewModel> _showSafetyCheckFaultCommand;
        public ICommand ShowSafetyCheckFaultCommand
        {
            get
            {
                return (_showSafetyCheckFaultCommand = _showSafetyCheckFaultCommand ?? new MvxCommand<DisplaySafetyCheckFaultItemViewModel>((f) => SafetyCheckFaultDetail(f)));
            }
        }

        #endregion Public Properties


        #region Private Methods

        private void SafetyCheckFaultDetail(DisplaySafetyCheckFaultItemViewModel fault)
        {
            if (!string.IsNullOrWhiteSpace(fault.FaultCheckComment))
                Mvx.Resolve<IUserInteraction>().Alert(fault.FaultCheckComment.Trim(new Char[] {' ', '-'}), null, string.Format("{0} - {1}", "Fault", fault.FaultCheckTitle));
        }

        private void GenerateSafetyCheckFaultItems(List<SafetyCheckFault> faults, bool isTrailer)
        {
            // Add the safety check item view models
            this.SafetyCheckFaultItemViewModels.AddRange(faults.Select(scf => new DisplaySafetyCheckFaultItemViewModel()
            {
                FaultCheckTitle = scf.Title,
                FaultCheckComment = (string.IsNullOrWhiteSpace(scf.Comment) ? "" : " - " + scf.Comment),
                FaultStatus = GetFaultStatusKey(scf.Status),
                FaultType = (isTrailer ? "TRL" : "VEH"),
            }));
        }

        private string GetFaultStatusKey(Enums.SafetyCheckStatus status)
        {
            switch (status)
            {
                case MWF.Mobile.Core.Enums.SafetyCheckStatus.NotSet:
                    return "";
                case MWF.Mobile.Core.Enums.SafetyCheckStatus.Passed:
                    return "FP";
                case MWF.Mobile.Core.Enums.SafetyCheckStatus.DiscretionaryPass:
                    return "DP";
                case MWF.Mobile.Core.Enums.SafetyCheckStatus.Failed:
                    return "F";
                default:
                    return "";
            }
        }

        #endregion Private Methods

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mainService.CurrentMobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Data may have changed.", null, "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Safety Check"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            var task = new Task<bool>(() => false);

            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = (_mainService.CurrentMobileData == null) ? Guid.Empty : _mainService.CurrentMobileData.ID };
            _navigationService.GoBack(navItem);

            return task;
        }
        #endregion IBackButtonHandler Implementation
    }
}
