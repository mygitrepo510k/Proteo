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
using System.Threading.Tasks;
using ZXing.Mobile;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class CheckInFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_CheckIn, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            this.Activity.ActionBar.Show();
            base.OnViewCreated(view, savedInstanceState);
            Task.Run(() => startScanning());
        }

        private async void startScanning()
        {
            Core.ViewModels.CheckInViewModel viewModel = (this.ViewModel as Core.ViewModels.CheckInViewModel);

            var scanner = new ZXing.Mobile.MobileBarcodeScanner(this.Activity);
            scanner.BottomText = "Scan the Check In QR code";
            scanner.TopText = "Proteo Mobile";
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE };
            await scanner.Scan(options).ContinueWith(t =>
            {
                if (t.Result != null)
                {
                    viewModel.ScannedQRCode = t.Result.Text;
                    viewModel.Message = "The Check In QR code has been successfully scanned. Click Continue to proceed.";
                }
                else viewModel.Message = "The Check In QR code could not be scanned. Go back and try again.";
            });
        }
    }
}