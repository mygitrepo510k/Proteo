using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
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
    public class OrderViewModel
        : BaseInstructionNotificationViewModel,
        IBackButtonHandler
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private IMainService _mainService;
        private MobileData _mobileData;
        private Item _order;
        private NavData<Item> _navData;

        #endregion Private Fields

        #region Construction

        public OrderViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
        }

        public void Init(NavData<Item> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _order = navData.Data;
            _mobileData = navData.GetMobileData();

        }


        #endregion Construction

        #region Public Properties

        public string OrderName { get { return "Order " + _order.ItemIdFormatted; } }

        public string OrderID { get { return _order.ItemIdFormatted; } }

        public string OrderLoadNo { get { return _order.Title; } }

        public string OrderDeliveryNo { get { return _order.DeliveryOrderNumber; } }

        public string OrderQuantity { get { return _order.Quantity; } }

        public string OrderWeight { get { return _order.Weight; } }

        public string OrderBusinessType { get { return _order.BusinessType; } }

        public string OrderGoodsType { get { return _order.GoodsType; } }

        public string ChangeOrderQuantityButtonLabel { get { return "Change Quantity"; } }

        public bool ChangeOrderQuantity { get { return _mobileData.Order.Type != Enums.InstructionType.Deliver; } }

        #endregion Public Properties

        #region Public Methods

        private MvxCommand<Item> _reviseQuantityOrderCommand;
        public ICommand ReviseQuantityOrderCommand
        {
            get
            {
                return (_reviseQuantityOrderCommand = _reviseQuantityOrderCommand ?? new MvxCommand<Item>(o => ReviseQuantity(o)));
            }
        }

        #endregion

        #region Private Methods

        private void ReviseQuantity(Item order)
        {
            _navigationService.MoveToNext(_navData);
        }

        private void GetMobileDataFromRepository(Guid parentID, Guid childID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(parentID);
            _order = _mobileData.Order.Items.First(i => i.ID == childID);
            _navData.OtherData["MobileData"] = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Order"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            var task = new Task<bool>(() => false);

            _navigationService.GoBack(_navData);

            return task;
        }
        #endregion IBackButtonHandler Implementation

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
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
