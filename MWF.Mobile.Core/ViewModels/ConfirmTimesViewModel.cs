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
            get
            {
                return (_mobileData.Order.Type == Enums.InstructionType.Collect || _mobileData.Order.Type == Enums.InstructionType.Deliver);
            }
        }

        public string ConfirmTimesButtonLabel
        {
            get
            {
                return "Confirm";
            }
        }
        #endregion Properties

        #region Private Methods
        private void AdvanceConfirmTimes()
        {
            _navData.Data.OnSiteDateTime = OnSiteDateTime;
            _navData.Data.CompleteDateTime = CompleteDateTime;
            
            _navigationService.MoveToNext(_navData);
        }

        #endregion

        public ConfirmTimesViewModel(INavigationService navigationService, IRepositories repositories, IInfoService infoService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _infoService = infoService;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
            CompleteDateTime = DateTime.Now;
            OnSiteDateTime = navData.Data.OnSiteDateTime;
        }

        private void RefreshPage(Guid ID)
        {
            _navData.ReloadInstruction(ID, _repositories);
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
            get
            {
                return (_confirmTimesCommand = _confirmTimesCommand?? new MvxCommand(() => AdvanceConfirmTimes()));
            }
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
                    RefreshPage(instructionID);
                }
                else
                {
                    if (this.IsVisible)
                    {
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                        _navigationService.GoToManifest();
                    }
                }
            }
        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            if (_mobileData.Order.Type == Enums.InstructionType.Deliver)
            {
                // Delivery, continue back using normal backstack mechanism
                return Task.FromResult(true);
            }
            else
            {
                    // Cellection, use custom back mapping action to skip the select trailer workflow
                    _navigationService.GoBack(_navData);
                    return Task.FromResult(false);
               
            }
        }

        #endregion IBackButtonHandler Implementation
    }
}
