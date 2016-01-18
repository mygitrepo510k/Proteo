using System;
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
using MWF.Mobile.Core.ViewModels.Extensions;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class ReviseQuantityViewModel : 
        BaseInstructionNotificationViewModel,
        IModalViewModel<bool>,
        IBackButtonHandler
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IInfoService _infoService;
        private readonly IDataChunkService _dataChunkService;

        private MobileApplicationDataChunkContentActivity _dataChunk;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private Item _order;

        #endregion Private Fields

        #region Construction

        public ReviseQuantityViewModel(
            INavigationService navigationService, 
            IRepositories repositories, 
            IInfoService infoService,
            IDataChunkService dataChunkService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _infoService = infoService;
            _dataChunkService = dataChunkService;
        }

        public void Init(Guid navID)
        {
            _navData = _navigationService.GetNavData<MobileData>(navID);
            this.MessageId = navID;
            _order = _navData.OtherData["Order"] as Item;
            _mobileData = _navData.Data;
            _dataChunk = _navData.GetAdditionalDataChunk(_mobileData);
            OrderQuantity = _order.Quantity;
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
            get { return (_reviseQuantityCommand = _reviseQuantityCommand ?? new MvxCommand(async () => await this.ReviseQuantityAsync())); }
        }

        #endregion Public Methods

        #region Private Methods

        public async Task ReviseQuantityAsync()
        {
            foreach (var order in _mobileData.Order.Items)
            {
                if (order.ID == _order.ID)
                {
                    order.Quantity = OrderQuantity;
                    order.Additional.ConfirmQuantity.Value = OrderQuantity;
                    _dataChunk.Data.Order.Add(order);
                }
            }
            //This value gets updated in HE.
            await _dataChunkService.SendDataChunkAsync(_dataChunk, _mobileData, _infoService.LoggedInDriver, _infoService.CurrentVehicle, updateQuantity: true);

            this.ReturnResult(true);
        }

        #endregion Private Methods

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressedAsync()
        {
            this.Cancel();
            return Task.FromResult(false);
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

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Revise Quantity"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region BaseInstructionNotificationViewModel

        public override Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage message)
        {
            var orderID = _order.ID;

            return this.RespondToInstructionNotificationAsync(message, _navData, () =>
            {
                _mobileData = _navData.Data;
                _order = _mobileData.Order.Items.First(i => i.ID == orderID);
                _navData.OtherData["Order"] = _order;
                OrderQuantity = _order.Quantity;
                RaiseAllPropertiesChanged();
            });
        }

        #endregion BaseInstructionNotificationViewModel

    }
}
