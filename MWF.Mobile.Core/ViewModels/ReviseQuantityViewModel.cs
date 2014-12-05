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

namespace MWF.Mobile.Core.ViewModels
{
    public class ReviseQuantityViewModel : BaseInstructionNotificationViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IMainService _mainService;
        private MobileData _mobileData;
        private Item _order;

        #endregion Private Fields

        #region Construction

        public ReviseQuantityViewModel(INavigationService navigationService, IRepositories repositories, IUserInteraction userInteraction, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
        }

        public void Init(NavItem<Item> item)
        {
            GetMobileDataFromRepository(item.ParentID, item.ID);
        }

        #endregion Construction

        #region Public Properties

        public string ReviseQuantityButtonLabel{ get { return "Update Quantity"; } }

        public string ReviseQuantityHeaderLabel { get { return "Revise Quantity"; } }

        public string OrderName { get { return "Order " + _order.ItemIdFormatted; } }

        private string _orderQuantity;
        public string OrderQuantity
        {
            get { return _orderQuantity; }
            set { _orderQuantity = value; RaisePropertyChanged(() => OrderQuantity); }
        }

        #endregion Public Properties

        #region Public Methods

        private MvxCommand _reviseQuantityCommand;
        public ICommand ReviseQuantityCommand
        {
            get
            {
                return (_reviseQuantityCommand = _reviseQuantityCommand ?? new MvxCommand(() => ReviseQuantity()));
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void ReviseQuantity()
        {

            foreach (var order in _mobileData.Order.Items)
            {
                if(order.ID == _order.ID)
                {
                    order.Quantity = OrderQuantity;
                    order.Additional.ConfirmQuantity.Value = OrderQuantity;
                    _mainService.CurrentDataChunkActivity.Order.Add(order);
                }
            }

            _mainService.CurrentMobileData = _mobileData;
            //This value gets updated in HE.
            _mainService.SendDataChunk(true);

            NavItem<Item> navItem = new NavItem<Item>() { ID = _order.ID, ParentID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);

        }

        private void GetMobileDataFromRepository(Guid parentID, Guid childID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(parentID);
            _order = _mobileData.Order.Items.First(i => i.ID == childID);
            OrderQuantity = _order.Quantity;
            RaiseAllPropertiesChanged();
            _mainService.CurrentMobileData = _mobileData;  
        }

        #endregion Private Methods

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Revise Quantity"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mainService.CurrentMobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID, _order.ID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel
    }
}
