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
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using MWF.Mobile.Core.Enums;

namespace MWF.Mobile.Android.Portable
{
    public class CustomUserInteraction : ICustomUserInteraction
    {
        public AlertDialog InstructionNotificationDialog;
        public AlertDialog CurrentInstructionNotificationDialog;
        public Vibrate Vibrate = new Vibrate();
        public Sound Sound = new Sound();

        protected Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        #region ICustomUserInteraction Members

        /// <summary>
        /// This method is called when new instructions that have been polled were added, updated or deleted.
        /// It causes a modal pop up to appear showing the new instructions and what action was taken to them.
        /// </summary>
        /// <param name="alteredInstructions">The list of instructions to display</param>
        /// <param name="done">The action that is taken when the postive button is pressed</param>
        /// <param name="title">Title of the modal</param>
        /// <param name="okButton">The text of the positive button</param>
        public void PopUpInstructionNotifaction(List<MobileData> alteredInstructions, Action done = null, string title = "", string okButton = "OK")
        {
            Application.SynchronizationContext.Post(ignored =>
            {
                if (CurrentActivity == null) return;

                //This closes the pop if its showing so it can reopen another, else it will play sound and vibrate first time.
                if (InstructionNotificationDialog != null && InstructionNotificationDialog.IsShowing)
                    InstructionNotificationDialog.Dismiss();
                else
                {
                    Vibrate.VibrateDevice();
                    Sound.Play();
                }


                var customView = CurrentActivity.LayoutInflater.Inflate(Resource.Layout.PopUp_InstructionNotification, null);

                InstructionGroupedListObject addInstructions = new InstructionGroupedListObject();
                InstructionGroupedListObject updateInstructions = new InstructionGroupedListObject();
                InstructionGroupedListObject deleteInstructions = new InstructionGroupedListObject();

                //Filter the instructions into SyncStates (Added, Updated, Deleted)
                foreach (var instruction in alteredInstructions)
                {
                    switch (instruction.SyncState)
                    {
                        case SyncState.Add:
                            addInstructions.Instructions.Add(instruction);
                            break;
                        case SyncState.Update:
                            updateInstructions.Instructions.Add(instruction);
                            break;
                        case SyncState.Delete:
                            deleteInstructions.Instructions.Add(instruction);
                            break;
                    }
                }


                List<InstructionGroupedListObject> inoList = new List<InstructionGroupedListObject>();
                List<string> headers = new List<string>();

                if (addInstructions.Instructions.Count > 0)
                {
                    inoList.Add(addInstructions);
                    headers.Add("Instructions added (" + addInstructions.Instructions.Count + ")");
                }

                if (updateInstructions.Instructions.Count > 0)
                {
                    inoList.Add(updateInstructions);
                    headers.Add(" Instructions updated (" + updateInstructions.Instructions.Count + ")");
                }

                if (deleteInstructions.Instructions.Count > 0)
                {
                    inoList.Add(deleteInstructions);
                    headers.Add("Instructions deleted (" + deleteInstructions.Instructions.Count + ")");
                }

                if (inoList.Count > 0)
                {
                    //Create the expandableListView to be displayed
                    ExpandableListView expandableListView = (ExpandableListView)customView.FindViewById(Resource.Id.instuctionList);
                    ExpandableListAdapter expandableListAdapter = new ExpandableListAdapter(CurrentActivity, headers, inoList);

                    expandableListView.SetAdapter(expandableListAdapter);

                    //Expand all the sections from the start.
                    for (int i = 0; i < expandableListAdapter.GroupCount; i++)
                    {
                        expandableListView.ExpandGroup(i);
                    }

                    //Create the dialog popup
                    var notificationDialog = new AlertDialog.Builder(CurrentActivity)
                        .SetView(customView)
                            .SetTitle(title)

                            //Prevents the user from closes the pop up by click on the sides.
                            .SetCancelable(false)
                            .SetPositiveButton(okButton, delegate
                            {
                                if (done != null)
                                    done();
                            });

                    InstructionNotificationDialog = notificationDialog.Create();
                    InstructionNotificationDialog.Show();
                }

            }, null);

        }

        /// <summary>
        /// This method is called when new instruction that has been polled and if the user is currently viewing that instruction then
        /// it causes a modal pop up to appear stating that the instruction has been altered and what action will be taken
        /// </summary>
        /// <param name="message">The message to appear in the pop up</param>
        /// <param name="done">The action that is taken when the postive button is pressed</param>
        /// <param name="title">Title of the modal</param>
        /// <param name="okButton">The text of the positive button</param>
        public void PopUpCurrentInstructionNotifaction(string message, Action done = null, string title = "", string okButton = "OK")
        {

            Application.SynchronizationContext.Post(ignored =>
           {
               if (CurrentActivity == null) return;

               //This closes the pop if its showing so it can reopen another, else it will play sound and vibrate first time.

               if (CurrentInstructionNotificationDialog != null && CurrentInstructionNotificationDialog.IsShowing)
                   CurrentInstructionNotificationDialog.Dismiss();

               var customView = CurrentActivity.LayoutInflater.Inflate(Resource.Layout.PopUp_CurrentInstructionNotification, null);
               var textView = (TextView)customView.FindViewById(Resource.Id.currentInstructionUpdateText);
               textView.Text = message;


               var notificationDialog = new AlertDialog.Builder(CurrentActivity)
                       .SetView(customView)
                           .SetTitle(title)

                           //Prevents the user from closes the pop up by click on the sides.
                           .SetCancelable(false)
                           .SetPositiveButton(okButton, delegate
                           {
                               if (done != null)
                                   done();
                           });

               CurrentInstructionNotificationDialog = notificationDialog.Create();
               CurrentInstructionNotificationDialog.Show();

           }, null);

        }

        public void PopUpImage(byte[] bytes, string message, Action done = null, string title = "", string okButton = "OK")
        {
            Application.SynchronizationContext.Post(ignored =>
            {

                if (CurrentActivity == null) return;

                var customView = CurrentActivity.LayoutInflater.Inflate(Resource.Layout.PopUp_Image, null);
                var imageView = (ImageView)customView.FindViewById(Resource.Id.popUpImageView);

                MvxInMemoryImageValueConverter converter = new MvxInMemoryImageValueConverter();
                var bitmap = (Bitmap)converter.Convert(bytes, typeof(Bitmap), null, null);
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

    }

    /// <summary>
    /// This object is used to display the grouped lists on the instruction notification Popup
    /// </summary>
    public class InstructionGroupedListObject
    {
        public InstructionGroupedListObject() { Instructions = new List<MobileData>(); }

        public List<MobileData> Instructions { get; set; }

    }
}