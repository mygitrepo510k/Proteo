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
using MWF.Mobile.Core.Converters;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class ConfirmQuantityFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_ConfimQuantity, null);
        }

        public override void OnResume()
        {
            base.OnResume();
            EditText revisePalletsText = (EditText)this.View.FindViewById(Resource.Id.revisePalletsText);
            EditText reviseCasesText = (EditText)this.View.FindViewById(Resource.Id.reviseCasesText);
            EditText reviseWeightText = (EditText)this.View.FindViewById(Resource.Id.reviseWeightText);

            this.ShowSoftKeyboard();
            //reviseQuantityText.RequestFocus();
            //this.ShowSoftKeyboard();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var submitButton = (Button)view.FindViewById(Resource.Id.submit);
            //var set = this.CreateBindingSet<ConfirmQuantityFragment, ConfirmQuantityViewModel>();
            //set.Bind(submitButton).For(b => b.Enabled).To(vm => vm.ConfirmQuantityEntered);
            //set.Apply();
        }
    }
}