using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class BaseFragment : MvxFragment
    {
        protected MvxFragment CurrentFragment;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var myActivity = (BaseActivityView)this.Activity;

            base.OnViewCreated(view, savedInstanceState);
        }
    }
}
