using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class BarcodeItemViewModel : MvxViewModel
    {
        private BarcodeViewModel _barcodeVM;

        public BarcodeItemViewModel(BarcodeViewModel barcodeVM)
        {
            _barcodeVM = barcodeVM;
        }

        private string _barcodeText;
        public string BarcodeText
        {
            get { return _barcodeText; }
            set { _barcodeText = value; RaisePropertyChanged(() => BarcodeText); }
        }

        private bool _isScanned;
        public bool IsScanned
        {
            get { return _isScanned; }
            set { _isScanned = value; RaisePropertyChanged(() => IsScanned); }
        }

        public string OrderID
        {
            get;
            set;
        }

        private Enums.ScanState _scanState;
        public Enums.ScanState ScanState
        {
            get { return _scanState; }
            set
            {
                if (value == Enums.ScanState.Scanned)
                {
                    this.IsScanned = true;
                    _scanState = value;
                    _barcodeVM.CheckBarcodeItemsStatus();
                }
                RaisePropertyChanged(() => ScanState);
            }
        }
    }
}
