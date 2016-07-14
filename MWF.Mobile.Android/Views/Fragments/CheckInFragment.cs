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
        private bool _scanning = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_CheckIn, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            this.Activity.ActionBar.Show();
            base.OnViewCreated(view, savedInstanceState);
            var scanAgainButton = view.FindViewById<Button>(Resource.Id.buttonScanAgain);
            scanAgainButton.Click += ScanAgainButton_Click;

            Task.Run(() => startScanning());
        }

        private void ScanAgainButton_Click(object sender, EventArgs e)
        {
            if(!_scanning) Task.Run(() => startScanning());
        }

        private async void startScanning()
        {
            _scanning = true;
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
                    if (Core.Helpers.CheckInOutQRCodeValidator.IsValidQRCode(t.Result.Text,
                        Core.Enums.CheckInOutActions.CheckIn))
                    {
                        viewModel.ScannedQRCode = t.Result.Text;
                        viewModel.Message = "The Check In QR code has been successfully scanned. Click Continue to proceed.";
                    }
                    else
                    {
                        viewModel.Message = "The scanned QR code is not valid. Please go back and try again.";
                    }
                }
                else viewModel.Message = "The Check In QR code could not be scanned. Please go back and try again.";

                _scanning = false;
            });
        }
    }
}