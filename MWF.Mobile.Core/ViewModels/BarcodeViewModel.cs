using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class BarcodeViewModel :
        BaseInstructionNotificationViewModel,
        IVisible
    {

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


        private IEnumerable<Barcode> _barcodes;
        public IEnumerable<Barcode> Barcodes
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

        private void CheckScanInput()
        {
            foreach (var barcode in Barcodes)
            {
                if(barcode.BarcodeText.Equals(BarcodeInput))
                {
                    barcode.IsScanned = true;
                }
            }
        }

        private void RefreshBarcodes()
        {

            var items = _mobileData.Order.Items.Select(i => i.Barcodes);
            Barcodes = items.Select(b => new Barcode { BarcodeText = b} );
        }

        public void IsVisible(bool isVisible)
        {
            if (isVisible) { }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {

        }

        public override string FragmentTitle
        {
            get { return "Collection Scan"; }
        }
    }
}
