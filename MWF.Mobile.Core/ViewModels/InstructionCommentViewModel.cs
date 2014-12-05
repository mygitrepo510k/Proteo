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
    public class InstructionCommentViewModel
        : BaseInstructionNotificationViewModel
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private IMainService _mainService;


        #endregion

        #region Construction

        public InstructionCommentViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
        }

        public void Init(NavItem<MobileData> item)
        {
            GetMobileDataFromRepository(item.ID);
        }


        #endregion

        #region Public Properties

        private MvxCommand _advanceInstructionCommentCommand;
        public ICommand AdvanceInstructionCommentCommand
        {
            get
            {
                return (_advanceInstructionCommentCommand = _advanceInstructionCommentCommand ?? new MvxCommand(() => AdvanceInstructionComment()));
            }
        }

        public string InstructionCommentButtonLabel
        {
            get
            {
                return ((_mobileData.Order.Type == Enums.InstructionType.Collect
                    && (_mobileData.Order.Additional.CustomerNameRequiredForCollection
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForCollection))
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver
                    && (_mobileData.Order.Additional.CustomerNameRequiredForDelivery
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery))) ? "Continue" : "Complete";
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

        private void AdvanceInstructionComment()
        {
            _mainService.CurrentDataChunkActivity.Comment = CommentText;

            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            RaiseAllPropertiesChanged();
            _mainService.CurrentMobileData = _mobileData;
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Comment"; }
        }

        #endregion

        #region BaseInstructionNotificationViewModel Overrides

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mainService.CurrentMobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel Overrides
    }
}
