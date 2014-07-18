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
using Android.Widget;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using SignaturePad;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class SafetyCheckSignatureFragment : MvxFragment
    {

        View _view;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            _view = this.BindingInflate(Resource.Layout.Fragment_SafetyCheckSignature, null);

            var doneButton = _view.FindViewById<Button>(Resource.Id.button_done);
            doneButton.Click += doneButton_Click;

            return _view;
        }

        void doneButton_Click(object sender, EventArgs e)
        {
            //TODO: temporary solution - would prefer to do this using data binding
            var signaturePad = _view.FindViewById<SignaturePadView>(Resource.Id.signature_view);
            signaturePad.SignaturePrompt.Text = string.Empty;

            var viewModel = (SafetyCheckSignatureViewModel)ViewModel;
            viewModel.Signature = new Core.Models.SignatureImage
            {
                Width = signaturePad.Width,
                Height = signaturePad.Height,
                Points = signaturePad.Points,
            };

            viewModel.DoneCommand.Execute(null);
        }

    }

}