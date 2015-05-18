using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
using Cirrious.MvvmCross.Droid.Views;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Android.Controls;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class PasscodeFragment : BaseFragment
    {

        private BindableProgress _bindableProgress;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_Passcode, null);
        }

        public override void OnResume()
        {
            base.OnResume();

            EditText passcodeText = (EditText)this.View.FindViewById(Resource.Id.editTextPasscode);
            passcodeText.RequestFocus();
            this.ShowSoftKeyboard();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            //Add the progress dialog to the view
            _bindableProgress = new BindableProgress(new ContextThemeWrapper(view.Context, Resource.Style.ProteoDialog));

            //Hide the action bar
            this.Activity.ActionBar.Hide();

            base.OnViewCreated(view, savedInstanceState);
            var set = this.CreateBindingSet<PasscodeFragment, PasscodeViewModel>();
            set.Bind(_bindableProgress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(_bindableProgress).For(p => p.Message).To(vm => vm.ProgressMessage);
            set.Bind(_bindableProgress).For(p => p.Title).To(vm => vm.ProgressTitle);
            set.Apply();

            var passCodeBox = view.FindViewById<EditText>(Resource.Id.editTextPasscode);
            passCodeBox.EditorAction += PassCodeBoxOnEditorAction;
        }

        private void PassCodeBoxOnEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId == ImeAction.Done)
            {
                var viewModel = this.ViewModel as PasscodeViewModel;
                
                viewModel.LoginCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}