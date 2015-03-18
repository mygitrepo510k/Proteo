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

namespace MWF.Mobile.Core.ViewModels
{
    public class MessageViewModel
        : BaseInstructionNotificationViewModel
    {

        #region Private Members

        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IMainService _mainService;
        private IRepositories _repositories;
        private INavigationService _navigationService;

        private MvxCommand _completeInstructionCommand;

        #endregion Private Members

        #region Construction

        public MessageViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _mainService = mainService;
            _repositories = repositories;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
        }

        #endregion Construction


        #region Public Properties

        public string MessageContentText { get { return _mobileData.Order.Items.First().Description; } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string PointDescription { get { return _mobileData.Order.Description; } }

        public string AddressLabelText { get { return "Address"; } }

        public string ProgressButtonText { get { return "Complete"; } }

        public bool isWithPoint { get { return _mobileData.Order.Addresses.Count > 0; } }

        public ICommand CompleteInstructionCommand
        {
            get
            {
                return (_completeInstructionCommand = _completeInstructionCommand ?? new MvxCommand(() => CompleteInstruction()));
            }
        }

        #endregion Public Properties


        #region Private Methods

        private void CompleteInstruction()
        {
            _navigationService.MoveToNext(_navData);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods


        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle { get { return (isWithPoint) ? "Message with a Point" : "Message"; } }

        #endregion  BaseFragmentViewModel Overrides
    }
}
