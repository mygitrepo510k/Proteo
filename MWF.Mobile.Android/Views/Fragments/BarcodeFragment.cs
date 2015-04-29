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
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class BarcodeFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_Barcode, null);
        }

        public override void OnResume()
        {
            base.OnResume();
            EditText barcodeInput = (EditText)this.View.FindViewById(Resource.Id.BarcodeInput);
            barcodeInput.RequestFocus();
            this.ShowSoftKeyboard();
        }

         public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var checksDoneButton = (Button)view.FindViewById(Resource.Id.ButtonCompleteScanning);
            var set = this.CreateBindingSet<BarcodeFragment, BarcodeViewModel>();
            set.Bind(checksDoneButton).For(b => b.Enabled).To(vm => vm.CanScanningBeCompleted);
            set.Apply();
        }
    }
    
}