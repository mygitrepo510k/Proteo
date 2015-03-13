using Cirrious.CrossCore;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionTrunkProceedViewModel
        : BaseInstructionNotificationViewModel
    {
        #region Private Members

        private MobileData _mobileData;
        private IMainService _mainService;
        private IRepositories _repositories;
        private INavigationService _navigationService;

        private MvxCommand _completeInstructionCommand;

        #endregion Private Members

        #region Construction

        public InstructionTrunkProceedViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _mainService = mainService;
            _repositories = repositories;
        }

        public void Init(NavItem<MobileData> item)
        {
            GetMobileDataFromRepository(item.ID);
            _mainService.OnManifestPage = false;
        }

        #endregion Construction

        #region Public Properties

        public string RunID { get { return _mobileData.Order.RouteTitle; } }

        public string ArriveDepartDateTime { get { return _mobileData.Order.Arrive.ToStringIgnoreDefaultDate(); } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string ArriveDepartLabelText { get { return (IsTrunkTo) ? "Arrive" : "Depart"; } }

        public string AddressLabelText { get { return "Address"; } }

        public string ProgressButtonText { get { return "Complete"; } }

        public bool IsTrunkTo { get { return _mobileData.Order.Type == Enums.InstructionType.TrunkTo; }}

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
            var navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            RaiseAllPropertiesChanged();
            _mainService.CurrentMobileData = _mobileData;
        }

        #endregion Private Methods

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mainService.CurrentMobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle { get { return (IsTrunkTo) ? "Trunk To" : "Proceed From"; }}

        #endregion  BaseFragmentViewModel Overrides

    }
}
