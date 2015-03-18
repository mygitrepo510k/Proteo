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
    public class InstructionOnSiteViewModel : BaseInstructionNotificationViewModel, IBackButtonHandler
    {

        
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IMainService _mainService;


        #endregion

        #region Construction

        public InstructionOnSiteViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _mainService = mainService;
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
                return ((_mobileData.Order.Type == Enums.InstructionType.Collect
                    && (_mobileData.Order.Additional.CustomerNameRequiredForCollection
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForCollection 
                    || _mobileData.Order.Additional.IsTrailerConfirmationEnabled))
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver
                    && (_mobileData.Order.Additional.CustomerNameRequiredForDelivery
                    || _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery))
                    || !_mobileData.Order.Items.FirstOrDefault().Additional.BypassCommentsScreen) ? "Continue" : "Complete";
            }
        }

        public string HeaderText { get { return "Select an order for further details"; } }

        #endregion

        #region Private Methods

        private void AdvanceInstructionOnSite()
        {
            _navigationService.MoveToNext(_navData);
        }

        private void ShowOrder(Item order)
        {
            NavData<Item> navData = new NavData<Item>() { Data = order};
            navData.OtherData["MobileData"] = _mobileData;
            navData.OtherData["DataChunk"] = navData.GetDataChunk();
            _navigationService.MoveToNext(navData);
        }

        private void RefreshPage(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _orderList = new ObservableCollection<Item>(_mobileData.Order.Items);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString() + " On Site"; }
        }

        #endregion

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
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => RefreshPage(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel Overrides
    }
}
