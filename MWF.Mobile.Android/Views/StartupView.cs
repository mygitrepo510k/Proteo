using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile;
using Android.Support.V4.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;
using support = Android.Support.V4.App;
using MWF.Mobile.Android.Helpers;

namespace MWF.Mobile.Android.Views
{

    [Activity(ScreenOrientation= ScreenOrientation.Portrait)]
    public class StartupView
        : BaseActivityView, Presenters.IFragmentHost
    {

        #region Construction

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Page_Startup);

            // Create the gateway queue timer service which will be available as a background service from here, regardless of activity.
            // Note: this does not actually start the timer, this is currently done in the MainViewModel once the user is fully logged in.
           // var queueTimerServiceIntent = new Intent(this, typeof(Services.GatewayQueueTimerService));
          //  StartService(queueTimerServiceIntent);

            // Create the gateway poll service which will be available as a background service from here, regardless of activity.
            // Note: this does not actually start the timer, this is currently done in the MainViewModel once the user is fully logged in.
           // var pollTimerServiceIntent = new Intent(this, typeof(Services.GatewayPollTimerService));
           // StartService(pollTimerServiceIntent);

            HockeyApp.LoginManager.Register(this, _hockeyAppID, _hockeyAppSecret, HockeyApp.LoginManager.LoginModeValidate, this.Class);
            CheckForUpdates();
        }

        protected override void OnPause()
        {
            base.OnPause();

            HockeyApp.UpdateManager.Unregister();
        }

        #endregion

        private void CheckForUpdates()
        {
            HockeyApp.UpdateManager.Register(this, _hockeyAppID, true);
        }

        #region Fragment Host

        public override IDictionary<Type, Type> SupportedFragmentViewModels
        {
            get { return _supportedViewModels; }
        }

        protected static IDictionary<Type, Type> _supportedViewModels = new Dictionary<Type, Type>
            {            
                { typeof(Core.ViewModels.PasscodeViewModel), typeof(Fragments.PasscodeFragment) },
                { typeof(Core.ViewModels.CustomerCodeViewModel), typeof(Fragments.CustomerCodeFragment)},
                { typeof(Core.ViewModels.VehicleListViewModel), typeof(Fragments.VehicleListFragment)},
                { typeof(Core.ViewModels.TrailerListViewModel), typeof(Fragments.TrailerListFragment)},
                { typeof(Core.ViewModels.AboutViewModel), typeof(Fragments.AboutFragment)},
                { typeof(Core.ViewModels.OdometerViewModel), typeof(Fragments.OdometerFragment)},
                { typeof(Core.ViewModels.SafetyCheckViewModel), typeof(Fragments.SafetyCheckFragment)},
			    { typeof(Core.ViewModels.SafetyCheckFaultViewModel), typeof(Fragments.SafetyCheckFaultFragment)},
                { typeof(Core.ViewModels.SafetyCheckSignatureViewModel), typeof(Fragments.SafetyCheckSignatureFragment) },
            };


        public override int FragmentHostID { get { return Resource.Id.fragment_host_startup; } }
        
 		#endregion
        

    }

}