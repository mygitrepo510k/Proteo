using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Extensions;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class ConfirmQuantityViewModel :
        BaseInstructionNotificationViewModel,
          IModalViewModel<bool>, IBackButtonHandler
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IInfoService _infoService;
        private readonly IDataChunkService _dataChunkService;


        private MobileApplicationDataChunkContentActivity _dataChunk;
        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private Item _item;
        private Order _order;

        #endregion Private Fields

        #region Construtor
        public ConfirmQuantityViewModel(
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
            _order = _navData.Data.Order;
            _item = _navData.Data.Order.Items[0];

            _mobileData = _navData.Data;

            if (_order.Type == Enums.InstructionType.Collect)
            {
                ConfirmCases = _item.ConfirmCasesForCollection;
                ConfirmPallets = _item.ConfirmPalletsForCollection;
                ConfirmWeight = _item.ConfirmWeightForCollection;
                ConfirmOther = _item.ConfirmOtherForCollection;
                if (ConfirmOther)
                {
                    ConfirmOtherText = _item.ConfirmOtherTextForCollection;
                }
            }

            if (_order.Type == Enums.InstructionType.Deliver)
            {
                ConfirmCases = _item.ConfirmCasesForDelivery;
                ConfirmPallets = _item.ConfirmPalletsForDelivery;
                ConfirmWeight = _item.ConfirmWeightForDelivery;
                ConfirmOther = _item.ConfirmOtherForDelivery;
                if (ConfirmOther)
                {
                    ConfirmOtherText = _item.ConfirmOtherTextForDelivery;
                }
            }

        }
        #endregion

        #region Public Properties

        public string ConfirmQuantityButtonLabel { get { return "Confirm Quantity"; } }
        public string ConfirmQuantityHeaderLabel { get { return "Confirm Quantity"; } }
        public string OrderName { get { return "Order " + _item.ItemIdFormatted; } }
        private string _caseCount;
        private string _palletCount;
        private string _weightCount;

        private string _cases;
        public string Cases
        {
            get { return _cases; }
            set { _cases = value; RaisePropertyChanged(() => Cases); }
        }
        private string _Pallets;
        public string Pallets
        {
            get { return _Pallets; }
            set { _Pallets = value; RaisePropertyChanged(() => Pallets); }
        }

        private string _weight;
        public string Weight
        {
            get { return _weight; }
            set { _weight = value; RaisePropertyChanged(() => Weight); }
        }

        private string _other;
        public string Other
        {
            get { return _other; }
            set { _other = value; RaisePropertyChanged(() => Other); }
        }

        private bool _confirmCases;
        private bool _confirmPallets;
        private bool _confirmWeight;
        private bool _confirmOther;
        private string _confirmOtherText;
        private bool _confirmQuantityEntered;

        public bool ConfirmCases
        {
            get { return _confirmCases; }
            set { _confirmCases = value; RaisePropertyChanged(() => ConfirmCases); CanContinue(); }
        }
        public bool ConfirmPallets
        {
            get { return _confirmPallets; }
            set { _confirmPallets = value; RaisePropertyChanged(() => ConfirmPallets); CanContinue(); }
        }
        public bool ConfirmWeight
        {
            get { return _confirmWeight; }
            set { _confirmWeight = value; RaisePropertyChanged(() => ConfirmWeight); CanContinue(); }
        }
        public bool ConfirmOther
        {
            get { return _confirmOther; }
            set { _confirmOther = value; RaisePropertyChanged(() => ConfirmOther); CanContinue(); }
        }

        public string ConfirmOtherText
        {
            get { return _confirmOtherText; }
            set { _confirmOtherText = value; RaisePropertyChanged(() => ConfirmOtherText); CanContinue(); }
        }

        public bool ConfirmQuantityEntered
        {
            get
            {
                return _confirmQuantityEntered;
            }
            set
            {
                _confirmQuantityEntered = value;
            }
        }
        public string ConfirmQuantityTitle
        {
            get { return  _item.DeliveryOrderNumber ?? _order.OrderId; }
        }
        #endregion

        #region Public Methods
        private IMvxCommand _confirmQuantityCommand;
        public ICommand ConfirmQuantityCommand
        {
            get { return (_confirmQuantityCommand = _confirmQuantityCommand ?? new MvxCommand(async () => await ConfirmQuantityAsync())); }
        }
        #endregion

        #region Private Methods
        private async Task<bool> ConfirmQuantityAsync()
        {
            bool isClaused = false;
            // check to make sure that what was entered matches what is on the order.
            if (ConfirmCases)
                isClaused =  (Cases != _item.Cases);
            if (ConfirmPallets && !isClaused)
                isClaused = (Pallets != _item.Pallets);
            if (ConfirmWeight && !isClaused)
                isClaused = (Weight != _item.Weight);

            if (_navData.OtherData.ContainsKey("IsClaused"))
                _navData.OtherData["IsClaused"] = isClaused;
            else
                _navData.OtherData.Add("IsClaused", isClaused);

            if (isClaused)
            {
                if (await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(message: "The numbers do not match are you sure you want to continue?", title: "Finished Quanities", okButton: "Yes"))
                {
                    await _navigationService.MoveToNextAsync(_navData);

                }
            }
            else
            {
                await _navigationService.MoveToNextAsync(_navData);
            }
        

            return isClaused;
        }

        private bool  CanContinue()
        {
            bool retVal = (ConfirmCases && !string.IsNullOrEmpty(Cases));
            retVal = retVal & (ConfirmPallets && !string.IsNullOrEmpty(Pallets));
            retVal = retVal & (ConfirmWeight&& !string.IsNullOrEmpty(Weight));
            retVal = retVal & (ConfirmOther&& !string.IsNullOrEmpty(Other));

            this.ConfirmQuantityEntered = retVal;

            return retVal;
        }
        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Confirm Quantity"; }
        }

        #endregion BaseFragmentViewModel Overrides


        #region BaseInstructionNotificationViewModel

        public override Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage message)
        {
            var orderID = _order.ID;

            return this.RespondToInstructionNotificationAsync(message, _navData, () =>
            {
                _mobileData = _navData.Data;
                _item = _mobileData.Order.Items.FirstOrDefault(i => i.ID == orderID);
                _navData.OtherData["Order"] = _item;
                RaiseAllPropertiesChanged();
            });
        }
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

        public async  Task<bool> OnBackButtonPressedAsync()
        {
                await _navigationService.GoBackAsync(_navData);
                return false;
        }

        #endregion BaseInstructionNotificationViewModel
    }
}
