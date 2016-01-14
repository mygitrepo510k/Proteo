﻿using Cirrious.CrossCore;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class ConfirmTimesViewModel : BaseInstructionNotificationViewModel, IBackButtonHandler
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IInfoService _infoService;


        #endregion

        #region Properties
        private DateTime _onSiteDateTime;
        public DateTime OnSiteDateTime
        {
            get { return _onSiteDateTime; }
            set { _onSiteDateTime = value; RaisePropertyChanged(() => OnSiteDateTime); }
        }

        private DateTime _completeDate;
        public DateTime CompleteDateTime
        {
            get { return _completeDate; }
            set { _completeDate = value; RaisePropertyChanged(() => CompleteDateTime); }
        }

        public bool ShowOnSiteConfirmation
        {
            get { return (_mobileData.Order.Type == Enums.InstructionType.Collect || _mobileData.Order.Type == Enums.InstructionType.Deliver); }
        }

        public string ConfirmTimesButtonLabel
        {
            get { return "Confirm"; }
        }

        private bool _isProgressing;
        public bool IsProgressing
        {
            get { return _isProgressing; }
            set { _isProgressing = value; RaisePropertyChanged(() => IsProgressing); }
        }

        #endregion Properties

        #region Private Methods

        public async Task AdvanceConfirmTimesAsync()
        {
            if (this.IsProgressing)
                return;

            this.IsProgressing = true;

            try
            {
                _navData.Data.OnSiteDateTime = OnSiteDateTime;
                _navData.Data.CompleteDateTime = CompleteDateTime;
                var additionalInstructions = _navData.GetAdditionalInstructions();

                foreach (var instruction in additionalInstructions)
                {
                    instruction.OnSiteDateTime = OnSiteDateTime;
                    instruction.CompleteDateTime = CompleteDateTime;
                }

                await _navigationService.MoveToNextAsync(_navData);
            }
            finally
            {
                this.IsProgressing = false;
            }
        }

        #endregion

        public ConfirmTimesViewModel(INavigationService navigationService, IRepositories repositories, IInfoService infoService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _infoService = infoService;
        }

        public void Init(Guid navID)
        {
            _navData = _navigationService.GetNavData<MobileData>(navID);
            _mobileData = _navData.Data;
            CompleteDateTime = DateTime.Now;
            OnSiteDateTime = _navData.Data.OnSiteDateTime;
        }

        private async Task RefreshPageAsync(Guid ID)
        {
            await _navData.ReloadInstructionAsync(ID, _repositories);
            _mobileData = _navData.Data;
            RaiseAllPropertiesChanged();
        }

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Confirm Times"; }
        }

        private MvxCommand _confirmTimesCommand;
        public ICommand ButtonAdvanceConfirmTimes
        {
            get { return (_confirmTimesCommand = _confirmTimesCommand?? new MvxCommand(async () => await this.AdvanceConfirmTimesAsync())); }
        }

        #endregion

        #region BaseInstructionNotificationViewModel Overrides

        public override async Task CheckInstructionNotificationAsync(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (_navData.GetAllInstructions().Any(i => i.ID == instructionID))
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    if (this.IsVisible)
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated.");

                    await this.RefreshPageAsync(instructionID);
                }
                else
                {
                    if (this.IsVisible)
                    {
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                       await  _navigationService.GoToManifestAsync();
                    }
                }
            }
        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region IBackButtonHandler Implementation

        public async Task<bool> OnBackButtonPressedAsync()
        {
            if (_mobileData.Order.Type == Enums.InstructionType.Deliver)
            {
                // Delivery, continue back using normal backstack mechanism
                return true;
            }
            else
            {
                // Collection, use custom back mapping action to skip the select trailer workflow
                await _navigationService.GoBackAsync(_navData);
                return false;
            }
        }

        #endregion IBackButtonHandler Implementation
    }
}
