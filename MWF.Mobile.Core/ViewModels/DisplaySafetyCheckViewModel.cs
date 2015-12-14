using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class DisplaySafetyCheckViewModel
        : BaseInstructionNotificationViewModel, 
        IBackButtonHandler
    {

        #region Private Members

        private IInfoService _infoService;
        private INavigationService _navigationService;
        private Repositories.IRepositories _repositories;
        private LatestSafetyCheck _latestSafetyCheckData;

        #endregion Private Members

        #region Construction
        public async override void Start()
        {
            base.Start();
            SafetyCheckFaultItemViewModels = new List<DisplaySafetyCheckFaultItemViewModel>();

            _latestSafetyCheckData = await _repositories.LatestSafetyCheckRepository.GetForDriverAsync(_infoService.LoggedInDriver.ID);

            //If there is no safety check data to view, then sends them back to where they came.
            if (_latestSafetyCheckData.VehicleSafetyCheck == null && _latestSafetyCheckData.TrailerSafetyCheck == null)
            {
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("A safety check profile for your vehicle and/or trailer has not been completed - Refer to your manual safety check.");
                await _navigationService.MoveToNextAsync(_navigationService.CurrentNavData);
                return;
            }

            if (_latestSafetyCheckData.VehicleSafetyCheck != null)
                GenerateSafetyCheckFaultItems(_latestSafetyCheckData.VehicleSafetyCheck.Faults, false);

            if (_latestSafetyCheckData.TrailerSafetyCheck != null)
                GenerateSafetyCheckFaultItems(_latestSafetyCheckData.TrailerSafetyCheck.Faults, true);
        }
        public DisplaySafetyCheckViewModel(IInfoService infoService, INavigationService navigationService, Repositories.IRepositories repositories)
        {
            _infoService = infoService;
            _navigationService = navigationService;
            _repositories = repositories;
            
        }

        #endregion Construction

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
            get { return _infoService.LoggedInDriver.DisplayName; }
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
                Mvx.Resolve<ICustomUserInteraction>().Alert(fault.FaultCheckComment.Trim(new Char[] {' ', '-'}), null, string.Format("{0} - {1}", "Fault", fault.FaultCheckTitle));
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

        public override async Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {

            if (_navigationService.CurrentNavData != null && _navigationService.CurrentNavData.GetMobileData() != null && _navigationService.CurrentNavData.GetMobileData().ID == instructionID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update && this.IsVisible)
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Data may have changed.", "This instruction has been updated");
                else
                {
                    if (this.IsVisible)
                    {
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                        await _navigationService.GoToManifestAsync();
                    }
                }
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

        public async Task<bool> OnBackButtonPressedAsync()
        {
            await _navigationService.GoBackAsync(_navigationService.CurrentNavData);
            return false;
        }

        #endregion IBackButtonHandler Implementation

    }
}
