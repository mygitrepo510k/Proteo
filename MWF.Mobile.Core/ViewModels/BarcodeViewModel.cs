using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class BarcodeViewModel :
        BaseInstructionNotificationViewModel,
        IVisible
    {
        #region Construction

        private MobileData _mobileData = null;
        private NavData<MobileData> _navData;

        private INavigationService _navigationService;

        public BarcodeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;


            RefreshBarcodes();

        }

        #endregion Construction

        #region Public Properties

        private ObservableCollection<BarcodeItemViewModel> _barcodes;
        public ObservableCollection<BarcodeItemViewModel> Barcodes
        {
            get { return _barcodes; }
            set { _barcodes = value; RaisePropertyChanged(() => Barcodes); }
        }

        private string _barcodeInput;
        public string BarcodeInput
        {
            get { return _barcodeInput; }
            set { _barcodeInput = value; RaisePropertyChanged(() => BarcodeInput); CheckScanInput(); }
        }

        private MvxCommand _completeScanningCommand;
        public ICommand CompleteScanningCommand
        {
            get
            {
                return (_completeScanningCommand = _completeScanningCommand ?? new MvxCommand(() => CompleteScanning()));
            }
        }

        public string CompleteButtonText
        {
            get
            {
                return "Continue";
            }
        }

        public bool CanScanningBeCompleted
        {
            get
            {
                bool allScansCompleted = true;

                foreach (var barcode in Barcodes)
                {
                    if (!allScansCompleted)
                        return allScansCompleted;

                    allScansCompleted = (barcode.ScanState != Enums.ScanState.NotScanned);
                }

                return allScansCompleted;
            }
            set
            {
                RaisePropertyChanged(() => CanScanningBeCompleted);
            }
        }

        #endregion Public Properties

        #region Private Methods

        private void CheckScanInput()
        {
            foreach (var barcode in Barcodes)
            {
                if (barcode.BarcodeText.Equals(BarcodeInput))
                {
                    barcode.ScanState = Enums.ScanState.Scanned;
                    BarcodeInput = string.Empty;
                }
            }
            RaisePropertyChanged(() => Barcodes);
        }

        private void RefreshBarcodes()
        {
            List<BarcodeItemViewModel> newBarcodeVM = new List<BarcodeItemViewModel>();
            var items = _mobileData.Order.Items;
            foreach (var item in items)
            {
                newBarcodeVM.AddRange(item.BarcodesList.Select(b => new BarcodeItemViewModel(this) { BarcodeText = b, OrderID = item.ItemIdFormatted }));
            }

           Barcodes = new ObservableCollection<BarcodeItemViewModel>(newBarcodeVM);
        }

        private void CompleteScanning()
        {
           
            var newScannedDelivery = new ScannedDelivery(){ Barcodes = this.Barcodes.Select(bvm => new Barcode { BarcodeText = bvm.BarcodeText, IsScanned = bvm.IsScanned, OrderID = bvm.OrderID }).ToList()};
             _navData.GetDataChunk().ScannedDelivery = newScannedDelivery;

            _navigationService.MoveToNext(_navData);
        }

        #endregion Private Methods

        #region Public Methods


        public void CheckBarcodeItemsStatus()
        {
            RaisePropertyChanged(() => CanScanningBeCompleted);
        }

        #endregion Public Methods




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

        #region BaseInstructionNotificationViewModel Overrides

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {

        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString() + " Scan"; }
        }

        #endregion BaseFragmentViewModel Overrides
    }
}
