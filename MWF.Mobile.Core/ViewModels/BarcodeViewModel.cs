using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels.Interfaces;
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

        public BarcodeViewModel()
        {
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
                if(barcode.BarcodeText.Equals(BarcodeInput))
                {
                    barcode.ScanState = Enums.ScanState.Scanned;
                }
            }
            RaisePropertyChanged(() => Barcodes);
        }

        private void RefreshBarcodes()
        {
            var items = _mobileData.Order.Items.Select(i => i.Barcodes);
            Barcodes = new ObservableCollection<BarcodeItemViewModel>(items.Select(b => new BarcodeItemViewModel(this) { BarcodeText = b }));
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
