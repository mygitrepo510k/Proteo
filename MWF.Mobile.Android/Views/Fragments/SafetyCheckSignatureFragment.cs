using System;
using System.Collections.Generic;
using System.IO;
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

using AndroidGraphics = Android.Graphics;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class SafetyCheckSignatureFragment : MvxFragment
    {

        SignaturePadView signaturePad;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.Fragment_SafetyCheckSignature, null);

            signaturePad = view.FindViewById<SignaturePadView>(Resource.Id.signature_view);
            signaturePad.SignaturePrompt.Text = string.Empty;
            
            var doneButton = view.FindViewById<Button>(Resource.Id.button_done);
            doneButton.Click += doneButton_Click;

            return view;
        }

        void doneButton_Click(object sender, EventArgs e)
        {
            //TODO: can we achieve this using data binding?
            var viewModel = (SafetyCheckSignatureViewModel)ViewModel;
            var image = signaturePad.GetImage(AndroidGraphics.Color.Black, AndroidGraphics.Color.White, false);

            using (var ms = new MemoryStream())
            {
                image.Compress(AndroidGraphics.Bitmap.CompressFormat.Png, 0, ms);
                viewModel.SignatureEncodedImage = Convert.ToBase64String(ms.ToArray());
            }

            viewModel.DoneCommand.Execute(null);
        }

    }

}