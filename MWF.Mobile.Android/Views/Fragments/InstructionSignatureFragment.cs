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
using SignaturePad;
using System.IO;
using AndroidGraphics = Android.Graphics;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class InstructionSignatureFragment : BaseFragment
    {
        SignaturePadView signaturePad;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.Fragment_InstructionSignature, null);

            signaturePad = view.FindViewById<SignaturePadView>(Resource.Id.signature_instructionView);
            signaturePad.StrokeColor = AndroidGraphics.Color.Black;
            signaturePad.BackgroundColor = AndroidGraphics.Color.Rgb(204, 207, 209); // Match the color of an EditText
            signaturePad.SignaturePrompt.Text = string.Empty;

            var doneButton = view.FindViewById<Button>(Resource.Id.ButtonAdvanceSignatureInstruction);
            doneButton.Click += this.DoneButton_Click;

            return view;
        }

        void DoneButton_Click(object sender, EventArgs e)
        {
            var viewModel = (InstructionSignatureViewModel)ViewModel;

            if (signaturePad.IsBlank)
                viewModel.CustomerSignatureEncodedImage = null;
            else
            {
                var image = signaturePad.GetImage(AndroidGraphics.Color.Black, AndroidGraphics.Color.White, false);

                using (var ms = new MemoryStream())
                {
                    image.Compress(AndroidGraphics.Bitmap.CompressFormat.Png, 0, ms);
                    viewModel.CustomerSignatureEncodedImage = Convert.ToBase64String(ms.ToArray());
                }
            }

            viewModel.InstructionDoneCommand.Execute(null);
        }

          
    }

}