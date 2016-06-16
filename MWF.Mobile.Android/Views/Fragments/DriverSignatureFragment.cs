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
using SignaturePad;
using MWF.Mobile.Core.ViewModels;
using AndroidGraphics = Android.Graphics;
using System.IO;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class DriverSignatureFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_DriverSignature, null);            
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            this.Activity.ActionBar.Hide();
            base.OnViewCreated(view, savedInstanceState);

            var completeButton = view.FindViewById<Button>(Resource.Id.buttonComplete);
            completeButton.Click += CompleteButton_Click;
        }

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            DriverSignatureViewModel viewModel = this.ViewModel as DriverSignatureViewModel;
            SignaturePadView signaturePad = this.View.FindViewById<SignaturePadView>(Resource.Id.driverSignatureView);
            if (signaturePad.IsBlank) viewModel.DriverSignature = null;
            else
            {
                var image = signaturePad.GetImage(AndroidGraphics.Color.Black,
                    AndroidGraphics.Color.White, 0.5f, shouldCrop: false);
                using (var ms = new MemoryStream())
                {
                    image.Compress(AndroidGraphics.Bitmap.CompressFormat.Png, 0, ms);
                    viewModel.DriverSignature = Convert.ToBase64String(ms.ToArray());
                }
                viewModel.CompleteCommand.Execute(null);
            }
        }
    }
}