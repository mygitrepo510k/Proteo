using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;
using Cirrious.MvvmCross.Droid.Views;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class CustomerCodeFragment : BaseFragment
    {
        private BindableProgress _bindableProgress;
        private View _view;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            _view = this.BindingInflate(Resource.Layout.Fragment_CustomerCode, null);
            return _view;
        }


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            //Add the progress dialog to the view


            _bindableProgress = new MWF.Mobile.Android.Views.BindableProgress(view.Context);

            //Hide the action bar
            this.Activity.ActionBar.Hide();
            
            base.OnViewCreated(view, savedInstanceState);
            var set = this.CreateBindingSet<CustomerCodeFragment, CustomerCodeViewModel>();
            set.Bind(_bindableProgress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(_bindableProgress).For(p => p.Message).To(vm => vm.ProgressMessage);
            set.Bind(_bindableProgress).For(p => p.Title).To(vm => vm.ProgressTitle);
            set.Apply();

            var customerCodeBox = view.FindViewById<EditText>(Resource.Id.CustomerCodeBox);
            customerCodeBox.EditorAction += CustomerCodeBoxOnEditorAction;

        }

        private void CustomerCodeBoxOnEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId == ImeAction.Done)
            {
                var viewModel = this.ViewModel as CustomerCodeViewModel;
                
                viewModel.EnterCodeCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}