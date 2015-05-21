﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionSignatureViewModel :
        BaseInstructionNotificationViewModel,
        IVisible
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly ICustomUserInteraction _userInteraction;
        private readonly IMainService _mainService;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;

        #endregion

        #region Construction

        public InstructionSignatureViewModel(INavigationService navigationService, IRepositories repositories, ICustomUserInteraction userInteraction, IMainService mobileApplicationDataChunkService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _userInteraction = userInteraction;
            _mainService = mobileApplicationDataChunkService;

        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
        }


        #endregion

        #region Public Properties

        public string InstructionSignatureButtonLabel { get { return "Complete"; } }

        public bool IsSignaturePadEnabled { get; set; }

        /// <summary>
        ///  If signature is required, the signature box stays active and you cannot press the 'Signature toggle' button.
        ///
        ///  If signature is NOT required, then the signature box is disabled by default, but can be turned on with the 'Signature toggle' button
        /// </summary>
        public bool IsSignatureToggleButtonEnabled
        {
            get
            {

                var deliveryOptions = _navData.GetWorseCaseDeliveryOptions();

                if ((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerSignatureRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && deliveryOptions.CustomerSignatureRequiredForDelivery))
                {
                    IsSignaturePadEnabled = true;
                    RaisePropertyChanged(() => SignatureToggleButtonLabel);
                    return false;
                }
                else
                {
                    IsSignaturePadEnabled = false;
                    RaisePropertyChanged(() => SignatureToggleButtonLabel);
                    return true;
                }
            }
        }

        public string SignatureToggleButtonLabel
        {
            get { return (IsSignaturePadEnabled) ? "Signature unavailable" : "Signature available"; }
        }

        public string InstructionSignaturePageHeader { get { return "I confirm this transaction"; } }

        private string _customerName;
        public string CustomerName
        {
            get { return _customerName; }
            set { _customerName = value; RaisePropertyChanged(() => CustomerName); }
        }

        private string _customerSignatureEncodedImage;
        public string CustomerSignatureEncodedImage
        {
            get { return _customerSignatureEncodedImage; }
            set { _customerSignatureEncodedImage = value; RaisePropertyChanged(() => CustomerSignatureEncodedImage); }
        }

        private MvxCommand _instructionDoneCommand;
        public ICommand InstructionDoneCommand
        {
            get
            {
                return (_instructionDoneCommand = _instructionDoneCommand ?? new MvxCommand(() => InstructionDone()));
            }
        }

        #endregion

        #region Private Methods

        private void InstructionDone()
        {

            var deliveryOptions = _navData.GetWorseCaseDeliveryOptions();

            if (((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerSignatureRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && deliveryOptions.CustomerSignatureRequiredForDelivery))
                    && string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage))
            {
                _userInteraction.Alert("Signature is required");
                return;
            }

            if (((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerNameRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && deliveryOptions.CustomerNameRequiredForDelivery))
                    && string.IsNullOrWhiteSpace(CustomerName))
            {
                _userInteraction.Alert("The signers name is required");
                return;
            }


            var dataChunks = _navData.GetAllDataChunks();
            foreach (var dataChunk in dataChunks)
            {
                dataChunk.Signature = new Models.Signature { Title = CustomerName, EncodedImage = CustomerSignatureEncodedImage };

                if (dataChunk.ScannedDelivery != null)
                {
                    dataChunk.ScannedDelivery.CustomerName = CustomerName;
                    dataChunk.ScannedDelivery.HasCustomerSigned = !string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage);
                }
            }

            _navigationService.MoveToNext(_navData);

        }

        private void RefreshPage(Guid ID)
        {
            _navData.ReloadInstruction(ID, _repositories);
            _mobileData = _navData.Data;
            RaiseAllPropertiesChanged();
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Sign for " + ((_mobileData.Order.Type == Enums.InstructionType.Collect) ? "Collection" : "Delivery"); }
        }

        #endregion

        #region BaseInstructionNotificationViewModel Overrides

        public override async Task CheckInstructionNotificationAsync(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (_navData.GetAllInstructions().Any(i => i.ID == instructionID))
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated.");
                    RefreshPage(instructionID);
                }
                else
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                    _navigationService.GoToManifest();
                }
            }
        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region IVisible

        public void IsVisible(bool isVisible)
        {
            if (isVisible) { }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        #endregion IVisible
    }
}
