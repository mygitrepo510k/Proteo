using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Cirrious.CrossCore;
using Android.Widget;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Plugins.PictureChooser.Droid;
using System.Threading.Tasks;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Android.Views;
using MWF.Mobile.Core.Models.Instruction;
using System.Collections.Generic;

namespace MWF.Mobile.Android.Portable
{
    public class CustomUserInteraction : ICustomUserInteraction
    {

        protected Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        #region ICustomUserInteraction Members

        public void PopUpInstructionNotifaction(List<MobileData> alteredInstructions, Action done = null, string title = "", string okButton = "OK")
        {
            Application.SynchronizationContext.Post(ignored =>
            {

                if (CurrentActivity == null) return;

                var customView = CurrentActivity.LayoutInflater.Inflate(Resource.Layout.PopUp_InstructionNotification, null);

                var header = (TextView)customView.FindViewById(Resource.Id.listHeader);
                header.SetText("Altered Instructions", TextView.BufferType.Normal);

                var listView = (ListView)customView.FindViewById(Resource.Id.instuctionList);

                //var groupView = (BindableGroupListView)customView.FindViewById(Resource.Id.instuctionList);

                //BindableGroupListAdapter grouplist = new BindableGroupListAdapter(CurrentActivity);

                //grouplist.ItemsSource = (IEnumerable<MobileData>)alteredInstructions;
                //grouplist.ItemTemplateId = Resource.Layout.Item_InstructionNotificationText;

                var listAdapter = new ArrayAdapter<MobileData>(CurrentActivity, Resource.Layout.Item_InstructionNotificationText, alteredInstructions);
                listView.Adapter = listAdapter;


                new AlertDialog.Builder(CurrentActivity)
                    .SetView(customView)
                    
                        .SetTitle(title)

                        .SetPositiveButton(okButton, delegate
                        {
                            if (done != null)
                                done();
                        })
                        .Show();

            }, null);
        }

        public Task PopUpInstructionNotifactionAsync(System.Collections.Generic.List<Core.Models.Instruction.MobileData> alteredInstructions, string title = "", string okButton = "OK")
        {
            throw new NotImplementedException();
        }

        public void PopUpImage(byte[] bytes, string message, Action done = null, string title = "", string okButton = "OK")
        {
            Application.SynchronizationContext.Post(ignored =>
            {

                if (CurrentActivity == null) return;

                var customView =  CurrentActivity.LayoutInflater.Inflate(Resource.Layout.PopUp_Image, null);
                var imageView = (ImageView) customView.FindViewById(Resource.Id.popUpImageView);

                MvxInMemoryImageValueConverter converter = new MvxInMemoryImageValueConverter();
                var bitmap = (Bitmap) converter.Convert(bytes, typeof(Bitmap), null, null);
                imageView.SetImageBitmap(bitmap);

                // Scale the image view to use maximum width
                SetImageViewSize(customView, imageView, bitmap);

                new AlertDialog.Builder(CurrentActivity) 
                    .SetView(customView)
                    .SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(okButton, delegate
                {
                    if (done != null)
                        done();
                })
                        .Show();
            }, null);
        }

        public Task PopUpImageAsync(byte[] bytes, string message, string title = "", string okButton = "OK")
        {
            var tcs = new TaskCompletionSource<object>();
            PopUpImage(bytes, message, () => tcs.SetResult(null), title, okButton);
            return tcs.Task;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the image view size. Since none of the android "ScaleType" enums deal with scaling
        /// an image up and maintaining aspect ratio, a code solution is required.
        /// </summary>
        /// <param name="parentView"></param>
        /// <param name="imageView"></param>
        /// <param name="bitmap"></param>
        private void SetImageViewSize(View parentView, ImageView imageView, Bitmap bitmap)
        {

            var metrics = parentView.Resources.DisplayMetrics;
            int height = metrics.HeightPixels;
            int width = metrics.WidthPixels - 200;

            float bmapHeight = bitmap.Height;
            float bmapWidth = bitmap.Width;

            float wRatio = width / bmapWidth;
            float hRatio = height / bmapHeight;

            float ratioMultiplier = wRatio;
            if (hRatio < wRatio)
            {
                ratioMultiplier = hRatio;
            }

            int newBmapWidth = (int)(bmapWidth * ratioMultiplier);
            int newBmapHeight = (int)(bmapHeight * ratioMultiplier);

            imageView.LayoutParameters.Width = newBmapWidth;
            imageView.LayoutParameters.Height = newBmapHeight;

        }
        


        #endregion


        public void OnClick(IDialogInterface dialog, int which)
        {
            throw new NotImplementedException();
        }

    }
}