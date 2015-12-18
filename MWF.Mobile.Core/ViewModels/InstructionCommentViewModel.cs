﻿using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionCommentViewModel
        : BaseInstructionNotificationViewModel, IBackButtonHandler
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IInfoService _infoService;

        #endregion

        #region Construction

        public InstructionCommentViewModel(INavigationService navigationService, IRepositories repositories, IInfoService infoService)
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
        }

        #endregion

        #region Public Properties

        private MvxCommand _advanceInstructionCommentCommand;
        public ICommand AdvanceInstructionCommentCommand
        {
            get { return (_advanceInstructionCommentCommand = _advanceInstructionCommentCommand ?? new MvxCommand(async () => await this.AdvanceInstructionCommentAsync())); }
        }

        public string InstructionCommentButtonLabel
        {
            get
            {
                var deliveryOptions = _navData.GetWorseCaseDeliveryOptions();

                return ((_mobileData.Order.Type == Enums.InstructionType.Collect
                    && (_mobileData.Order.Additional.CustomerNameRequiredForCollection
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForCollection))
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver
                    && (deliveryOptions.CustomerNameRequiredForDelivery
                    || deliveryOptions.CustomerSignatureRequiredForDelivery))) ? "Continue" : "Complete";
            }
        }

        public string InstructionCommentPageHeader { get { return "Comment Screen"; } }

        private string _commentText;
        public string CommentText
        {
            get { return _commentText; }
            set { _commentText = value; RaisePropertyChanged(() => CommentText); }
        }


        #endregion

        #region Private Methods

        public Task AdvanceInstructionCommentAsync()
        {
            var dataChunks = _navData.GetAllDataChunks();

            foreach (var dataChunk in dataChunks)
            {
                dataChunk.Comment = CommentText;
            }

            return _navigationService.MoveToNextAsync(_navData);
        }

        private async Task RefreshPageAsync(Guid ID)
        {
            await _navData.ReloadInstructionAsync(ID, _repositories);
            _mobileData = _navData.Data;
            RaiseAllPropertiesChanged();
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Comment"; }
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
                        await _navigationService.GoToManifestAsync();
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
