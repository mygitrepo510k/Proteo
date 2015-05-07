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
    public class LaunchPhone: ILaunchPhone
    {

        protected Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        public void Launch()
        {
            Intent i = new Intent(Intent.ActionDial);

            this.CurrentActivity.StartActivity(i);
            
        }
    }
}