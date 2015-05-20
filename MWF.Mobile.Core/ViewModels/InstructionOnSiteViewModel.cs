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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionOnSiteViewModel : 
        BaseInstructionNotificationViewModel, 
        IBackButtonHandler, 
        IVisible
    {

        
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IMainService _mainService;
        private IDataChunkService _dataChunkService;


        #endregion

        #region Construction

        public InstructionOnSiteViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService, IDataChunkService dataChunkService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
            _dataChunkService = dataChunkService;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
            _orderList = new ObservableCollection<Item>(_mobileData.Order.Items);
        }

        #endregion

        #region Public Properites

        private MvxCommand _advanceInstructionOnSiteCommand;
        public ICommand AdvanceInstructionOnSiteCommand
        {
            get
            {
                return (_advanceInstructionOnSiteCommand = _advanceInstructionOnSiteCommand ?? new MvxCommand(() => AdvanceInstructionOnSite()));
            }
        }

        private MvxCommand<Item> _showInstructionOrderCommand;
        public ICommand ShowInstructionOrderCommand
        {
            get
            {
                return(_showInstructionOrderCommand = _showInstructionOrderCommand ?? new MvxCommand<Item>(o => ShowOrder(o)));
            }
        }

        private MvxCommand _addDeliveriesCommand;
        public ICommand AddDeliveriesCommand
        {
            get
            {
                return (_addDeliveriesCommand = _addDeliveriesCommand ?? new MvxCommand(() => AddDeliveries()));
            }
        }

        private ObservableCollection<Item> _orderList;
        public ObservableCollection<Item> OrderList
        {
            get { return _orderList; }
            set { _orderList = value; RaisePropertyChanged(() => OrderList); }
        }

        public string InstructionCommentButtonLabel
        {
            get
            {
                var deliveryOptions = _navData.GetWorseCaseDeliveryOptions();

                return ((_mobileData.Order.Type == Enums.InstructionType.Collect
                    && (_mobileData.Order.Additional.CustomerNameRequiredForCollection
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForCollection 
                    || _mobileData.Order.Additional.IsTrailerConfirmationEnabled))
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver
                    && (deliveryOptions.CustomerNameRequiredForDelivery
                    || deliveryOptions.CustomerSignatureRequiredForDelivery
                    || deliveryOptions.BarcodeScanRequiredForDelivery
                    || !deliveryOptions.BypassCommentsScreen
                    || !deliveryOptions.BypassCleanClausedScreen))) ? "Continue" : "Complete";
            }
        }

        public string HeaderText { get { return "Select an order for further details"; } }

        public string AddDeliveriesButtonLabel
        {
            get
            {
                return "Add/Remove Deliveries";
            }
        }

        public bool IsDelivery
        {
            get
            {
                return _mobileData.Order.Type == Enums.InstructionType.Deliver;
            }
        }

        #endregion

        #region Private Methods

        private void AdvanceInstructionOnSite()
        {
            SendAdditionalInstructionOnSiteDataChunks();
            _navigationService.MoveToNext(_navData);
        }

        private void ShowOrder(Item order)
        {
            NavData<MobileData> navData = new NavData<MobileData>();

            if (order.OrderId == _navData.Data.Order.ID)
            {
                navData.Data = _mobileData;
                navData.OtherData["DataChunk"] = navData.GetDataChunk();
            }
            else
            {
                navData.Data = _navData.GetAdditionalInstructions().FirstOrDefault(md => md.Order.ID == order.OrderId);
                navData.OtherData["DataChunk"] = navData.GetAdditionalDataChunk(navData.Data);
            }

            navData.OtherData["Order"] = order;

            _navigationService.ShowModalViewModel<OrderViewModel, bool>(this, navData, (modified) =>
                {
                    if (modified)
                    {
                       
                    }
                }               
            );
        }


        /// <summary>
        /// Ensures that all the additional instructions added send "onSite" data chunks
        /// </summary>
        private void SendAdditionalInstructionOnSiteDataChunks()
        {
            var additionalInstructions = _navData.GetAdditionalInstructions();

            foreach (var additionalInstruction in additionalInstructions)
            {
                if (additionalInstruction.ProgressState != Enums.InstructionProgress.OnSite)
                {
                    additionalInstruction.ProgressState = Enums.InstructionProgress.OnSite;
                    _dataChunkService.SendDataChunk(_navData.GetAdditionalDataChunk(additionalInstruction), additionalInstruction, _mainService.CurrentDriver, _mainService.CurrentVehicle);
                }
            }

        }

        private void AddDeliveries()
        {

            _navigationService.ShowModalViewModel<InstructionAddDeliveriesViewModel, bool>(this, _navData, (modified) =>
            {
                if (modified)
                {
                    RefreshOrders();
                }
            });
        }

        private void RefreshOrders()
        {
            List<Item> newOrderList = new List<Item>(_mobileData.Order.Items);
            var additionalInstructions = _navData.GetAdditionalInstructions();
            foreach (var additionalInstruction in additionalInstructions)
            {
                newOrderList.AddRange(additionalInstruction.Order.Items);
            }

            this.OrderList = new ObservableCollection<Item>(newOrderList);
        }

        private void RefreshPage(Guid ID)
        {

            _navData.ReloadInstruction(ID, _repositories);
            _mobileData = _navData.Data;

            RefreshOrders();
            RaiseAllPropertiesChanged();
        }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString() + " On Site"; }
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

        #region BaseInstructionNotificationViewModel Overrides

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (_navData.GetAllInstructions().Any(i => i.ID == instructionID))
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Now refreshing the page.", () => RefreshPage(instructionID), "This instruction has been updated.", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been deleted.");
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
