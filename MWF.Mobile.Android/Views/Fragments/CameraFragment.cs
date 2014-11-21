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
    public class CameraFragment : BaseFragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.Fragment_Camera, null);

            var doneButton = view.FindViewById<Button>(Resource.Id.doneButton);
            doneButton.Click += DoneButton_Click;

            return view;
        }

        public void DoneButton_Click(object sender, EventArgs e)
        {
            FragmentManager.PopBackStack();
        }
    }
}
