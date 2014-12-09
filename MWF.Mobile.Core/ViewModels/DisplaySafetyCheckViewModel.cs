using Cirrious.CrossCore;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class DisplaySafetyCheckViewModel
        : BaseInstructionNotificationViewModel
    {
        private IMainService _mainService;
        private INavigationService _navigationService;
        private Repositories.IRepositories _repositories;
        private IStartupService _startupService;

        public DisplaySafetyCheckViewModel(IMainService mainService, INavigationService navigationService, IStartupService startupService, Repositories.IRepositories repositories)
        {
            //Safety check data is within this service.

            _mainService = mainService;
            _navigationService = navigationService;
            _repositories = repositories;
            _startupService = startupService;

            var driverSafetyCheck = _repositories.LatestSafetyCheckRepository.GetForDriver(_mainService.CurrentDriver.ID);
            SafetyCheckFaultItemViewModels = new List<DisplaySafetyCheckFaultItemViewModel>();

            if (driverSafetyCheck.VehicleSafetyCheck != null)
                GenerateSafetyCheckFaultItems(driverSafetyCheck.VehicleSafetyCheck.Faults, false);

            if (driverSafetyCheck.TrailerSafetyCheck != null)
                GenerateSafetyCheckFaultItems(driverSafetyCheck.TrailerSafetyCheck.Faults, true);

        }

        #region Public Properties

        public string VehicleRegistration
        {
            get { return "Vehicle: " + ((_startupService.CurrentVehicle != null) ? _startupService.CurrentVehicleSafetyCheckData.VehicleRegistration : ""); }
        }

        public string TrailerRegistration
        {
            get { return "Trailer: " + ((_startupService.CurrentTrailer != null) ? _startupService.CurrentTrailerSafetyCheckData.VehicleRegistration : ""); }
        }

        public string VehicleSafetyCheckStatus
        {
            get { return "Checked: " + ((_startupService != null) ? _startupService.CurrentVehicleSafetyCheckData.EffectiveDate.ToString("g") : ""); }
        }

        public string TrailerSafetyCheckStatus
        {
            get { return "Checked: " + ((_startupService.CurrentTrailer != null) ? _startupService.CurrentTrailerSafetyCheckData.EffectiveDate.ToString("g") : ""); }
        }

        public string SafetyCheckStatusKey
        {
            get { return "* FP = Full Pass; DP = Discretionary Pass; F = Fail"; }
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

        #endregion Public Properties

        private void GenerateSafetyCheckFaultItems(List<SafetyCheckFault> faults, bool isTrailer)
        {
            // Add the safety check item view models
            this.SafetyCheckFaultItemViewModels.AddRange(faults.Select(scf => new DisplaySafetyCheckFaultItemViewModel()
            {
                FaultCheckTitleAndComment = (string.IsNullOrWhiteSpace(scf.Comment) ? scf.Title : string.Format("{0} - {1}", scf.Title, scf.Comment)),
                FaultStatus = GetFaultStatusKey(scf.Status),
                FaultType = (isTrailer ? "TRL " : "VEH "),
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
    }
}
