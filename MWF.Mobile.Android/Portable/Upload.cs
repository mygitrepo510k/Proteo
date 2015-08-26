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
using Android.Net;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;

namespace MWF.Mobile.Android.Portable
{
    class Upload : IUpload
    {
        protected Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        public bool UploadFile(System.Uri address,string username, string password, string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            WebClient wc = new WebClient();
            wc.Credentials = new NetworkCredential(username, password);
            bool success = false;
            try
            {
                wc.UploadFile(address, path);
                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return success;


        }

    }
}