using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Android.Views.Fragments;


namespace MWF.Mobile.Android.Views
{

    [Activity]
    public abstract class BaseActivityView
        : MvxActivity
    {

        protected abstract Type GetFragmentTypeForViewModel(Type viewModelType);

        

        protected BaseActivityViewModel BaseActivityViewModel
        {
            get { return (BaseActivityViewModel)this.ViewModel; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Populate FrameLayout with initial viewmodel fragment
            var initialViewModel = this.BaseActivityViewModel.InitialViewModel;
            var fragmentType = GetFragmentTypeForViewModel(initialViewModel.GetType());

            var fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
            fragment.ViewModel = initialViewModel;

            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_host, fragment);
            transaction.Commit();

        }
    }
}