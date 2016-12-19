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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

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
        public ObservableCollection<ItemConfirmQuantityViewModel> _items = new ObservableCollection<ItemConfirmQuantityViewModel>();
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
            var additionalInstructions = _navData.GetAdditionalInstructions();
            var mobileDataList = new List<MobileData>();
            mobileDataList.Add(_navData.Data);
            if (additionalInstructions.Count() > 0)
                mobileDataList.AddRange(additionalInstructions);

            foreach (var mobileData in mobileDataList)
            {
                foreach (Item item in mobileData.Order.Items)
                {
                    if (_order.Type == Enums.InstructionType.Collect &&
                        (item.ConfirmCasesForCollection || item.ConfirmOtherForCollection || item.ConfirmPalletsForCollection || item.ConfirmWeightForCollection))
                    {
                        this.Items.Add(new ItemConfirmQuantityViewModel(item, _order.Type));
                    }
                    if (_order.Type == Enums.InstructionType.Deliver &&
                        (item.ConfirmCasesForDelivery || item.ConfirmOtherForDelivery || item.ConfirmPalletsForDelivery || item.ConfirmWeightForDelivery))
                    {
                        this.Items.Add(new ItemConfirmQuantityViewModel(item, _order.Type));
                    }
                }
            }

            

        }
        #endregion

        #region Public Properties

        public string ConfirmQuantityButtonLabel { get { return "Confirm Quantity"; } }
        public string ConfirmQuantityHeaderLabel { get { return "Confirm Quantity"; } }
       
        public ObservableCollection<ItemConfirmQuantityViewModel> Items
        {
            get { return _items; }
            set { _items = value;  RaisePropertyChanged(() => Items); }
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
            foreach (var item in Items)
            {
                isClaused = item.IsClaused;
                if (isClaused) // no need to loop through all if anyone is claused.
                    break;
            }
            // check to make sure that what was entered matches what is on the order.
            
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
