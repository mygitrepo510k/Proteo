using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using ZXing.Mobile;
using System.Threading.Tasks;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class QRCodeFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {            
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_QRCode, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Task.Run(() => startScanning());
        }

        private async void startScanning()
        {
#if DEBUG
            (this.ViewModel as Core.ViewModels.QRCodeViewModel).ScannedQRCode =
                @"{deviceId:1, imei:'123456789012345',phoneNumber:'8989799898',actionPerformed:1,driverId:5}";
#else
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            scanner.UseCustomOverlay = true;
            scanner.CustomOverlay = this.Activity.FindViewById(Resource.Id.qrcodeScanner);
            var result = await scanner.Scan();

            if (result != null)
                (this.ViewModel as Core.ViewModels.QRCodeViewModel).ScannedQRCode = result.Text;
#endif
        }
    }
}