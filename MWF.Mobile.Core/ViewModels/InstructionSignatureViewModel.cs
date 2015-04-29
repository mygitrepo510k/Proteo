using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private readonly IUserInteraction _userInteraction;
        private readonly IMainService _mainService;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;

        #endregion

        #region Construction

        public InstructionSignatureViewModel(INavigationService navigationService, IRepositories repositories, IUserInteraction userInteraction, IMainService mobileApplicationDataChunkService)
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

                if ((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerSignatureRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery))
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

            if (((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerSignatureRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery))
                    && string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage))
            {
                _userInteraction.Alert("Signature is required");
                return;
            }

            if (((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerNameRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && _mobileData.Order.Additional.CustomerNameRequiredForDelivery))
                    && string.IsNullOrWhiteSpace(CustomerName))
            {
                _userInteraction.Alert("The signers name is required");
                return;
            }

            _navData.GetDataChunk().Signature = new Models.Signature { Title = CustomerName, EncodedImage = CustomerSignatureEncodedImage };

            //This indicates that the instruction has been scanned
            if (_navData.GetDataChunk().ScannedDelivery != null)
            {
                _navData.GetDataChunk().ScannedDelivery.CustomerName = CustomerName;
                _navData.GetDataChunk().ScannedDelivery.HasCustomerSigned = !string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage);
            }

            _navigationService.MoveToNext(_navData);

        }

        private void RefreshPage(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
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

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Now refreshing the page.", () => RefreshPage(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
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
