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
using MWF.Mobile.Core.Portable;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;

namespace MWF.Mobile.Android.Portable
{
    public class CheckForSoftwareUpdates
        : ICheckForSoftwareUpdates
    {
        protected const string _hockeyAppID = "7cf7fc0dba2bf468cec55795c004d77d";

        protected Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        public void Check()
        {
            HockeyApp.UpdateManager.Unregister();
            HockeyApp.UpdateManager.Register(CurrentActivity, _hockeyAppID, true);
        }
    }
}