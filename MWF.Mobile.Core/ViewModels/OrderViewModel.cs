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
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.ViewModels
{
    public class OrderViewModel
        : BaseInstructionNotificationViewModel,
        IModalViewModel<bool>,
        IVisible,
        IBackButtonHandler
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IConfigRepository _configRepository;

        private IMainService _mainService;
        private MobileData _mobileData;
        private MWFMobileConfig _mobileConfig;
        private Item _order;
        private NavData<MobileData> _navData;

        #endregion Private Fields

        #region Construction

        public OrderViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
            _configRepository = repositories.ConfigRepository;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            this.MessageId = navData.NavGUID;
            _navData = navData;
            _order = navData.OtherData["Order"] as Item;
            _mobileData = navData.Data;
            _mobileConfig = _configRepository.GetByID(_mobileData.CustomerId);
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

        public bool ChangeOrderQuantity { get { return _mobileConfig.QuantityIsEditable && _mobileData.Order.Type != Enums.InstructionType.Deliver; } }

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
            _navigationService.ShowModalViewModel<ReviseQuantityViewModel, bool>(this, _navData, (confirmed) => {});

            //_navigationService.MoveToNext(_navData);
        }

        private void GetMobileDataFromRepository(Guid parentID, Guid childID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(parentID);
            _order = _mobileData.Order.Items.First(i => i.ID == childID);
            _navData.OtherData["Order"] = _order;
            _navData.Data = _mobileData;
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

            this.Cancel();

            return task;

        }

        #endregion IBackButtonHandler Implementation

        #region IModalViewModel

        public Guid MessageId { get; set; }

        public void Cancel()
        {
            ReturnResult(default(bool));
        }

        public void ReturnResult(bool result)
        {
            var message = new ModalNavigationResultMessage<bool>(this, MessageId, result);

            this.Messenger.Publish(message);
            this.Close(this);
        }

        #endregion

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID, _order.ID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

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
