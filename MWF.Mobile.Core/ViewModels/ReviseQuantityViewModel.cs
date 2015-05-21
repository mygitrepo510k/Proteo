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
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class ReviseQuantityViewModel : 
        BaseInstructionNotificationViewModel,
        IModalViewModel<bool>,
        IBackButtonHandler,
        IVisible
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IMainService _mainService;
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
            IMainService mainService,
            IDataChunkService dataChunkService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
            _dataChunkService = dataChunkService;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            this.MessageId = navData.NavGUID;
            _navData = navData;
            _order = navData.OtherData["Order"] as Item;
            _mobileData = navData.Data;
            _dataChunk = navData.GetAdditionalDataChunk(_mobileData);
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
                    _dataChunk.Data.Order.Add(order);
                }
            }
            //This value gets updated in HE.
            _dataChunkService.SendDataChunk(_dataChunk, _mobileData, _mainService.CurrentDriver, _mainService.CurrentVehicle, updateQuantity: true);

            this.ReturnResult(true);

        }

        private void GetMobileDataFromRepository(Guid parentID, Guid childID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(parentID);
            _order = _mobileData.Order.Items.First(i => i.ID == childID);
            _navData.Data = _mobileData;
            _navData.OtherData["Order"] = _order;
            OrderQuantity = _order.Quantity;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods

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

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Revise Quantity"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region BaseInstructionNotificationViewModel

        public override async Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated");
                    GetMobileDataFromRepository(instructionID, _order.ID);
                }
                else
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted");
                    _navigationService.GoToManifest();
                }
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
