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
using Android.Support.V4.App;
using MWF.Mobile.Android.Helpers;

namespace MWF.Mobile.Android.Views
{

    [Activity(ScreenOrientation= ScreenOrientation.Portrait)]
    public class StartupView
        : BaseActivityView, Presenters.IFragmentHost
    {

        private DrawerLayout _drawer;
        private MvxListView _drawerList;
        private ActionBarDrawerToggle _drawerToggle;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Page_Startup);

            //Navigation Draw
            _drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            _drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

            this.ActionBar.SetDisplayHomeAsUpEnabled(true);
            this.ActionBar.SetHomeButtonEnabled(true);


            _drawerToggle = new CustomActionBarDrawerToggle(this, this._drawer, Resource.Drawable.ic_drawer_light, Resource.String.drawer_open,
                                                      Resource.String.drawer_close);

            _drawer.DrawerOpened += delegate
            {
                this.InvalidateOptionsMenu();
            };

            _drawer.DrawerClosed += delegate
            {
                this.InvalidateOptionsMenu();
            };

            this._drawer.SetDrawerListener(this._drawerToggle);

            // Create the gateway queue timer service which will be available as a background service from here, regardless of activity.
            // Note: this does not actually start the timer, this is currently done in the MainViewModel once the user is fully logged in.
            var queueTimerServiceIntent = new Intent(this, typeof(Services.GatewayQueueTimerService));
            StartService(queueTimerServiceIntent);

        }

        protected override Type GetFragmentTypeForViewModel(Type viewModelType)
        {
            return _supportedFragmentViewModels[viewModelType];
        }


		#region Fragment host

        private static IDictionary<Type, Type> _supportedFragmentViewModels = new Dictionary<Type, Type>
        {
            { typeof(Core.ViewModels.PasscodeViewModel), typeof(Fragments.PasscodeFragment) },
            { typeof(Core.ViewModels.CustomerCodeViewModel), typeof(Fragments.CustomerCodeFragment)},
            { typeof(Core.ViewModels.VehicleListViewModel), typeof(Fragments.VehicleListFragment)},
            { typeof(Core.ViewModels.TrailerListViewModel), typeof(Fragments.TrailerListFragment)},
            { typeof(Core.ViewModels.AboutViewModel), typeof(Fragments.AboutFragment)},
            {typeof(Core.ViewModels.OdometerViewModel), typeof(Fragments.OdometerFragment)},
            { typeof(Core.ViewModels.SafetyCheckViewModel), typeof(Fragments.SafetyCheckFragment)},
			{ typeof(Core.ViewModels.SafetyCheckFaultViewModel), typeof(Fragments.SafetyCheckFaultFragment)},
            { typeof(Core.ViewModels.SafetyCheckSignatureViewModel), typeof(Fragments.SafetyCheckSignatureFragment) },
        };

        public bool Show(MvxViewModelRequest request)
        {
            // At this point simply display any supported fragment in the FrameLayout.
            // In future we may change this for different device form factors, for example to display tiled fragments.
            // For this reason I have duplicated this code in both MainView and StartupView, rather than abstracting
            // to a base class, since we may wish to handle fragements differently in each at some point.
            MvxFragment fragment = null;

            if (_supportedFragmentViewModels.ContainsKey(request.ViewModelType))
            {
                var fragmentType = _supportedFragmentViewModels[request.ViewModelType];
                fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
                fragment.LoadViewModelFrom(request, null);
            }

            if (fragment == null)
                return false;

            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_host, fragment);
            transaction.AddToBackStack(null);
            transaction.Commit();

            return true;
        }

 		#endregion Fragment host


        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.main_activity_actions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(global::Android.Views.IMenu menu)
        {
            var drawerOpen = this._drawer.IsDrawerOpen(this._drawerList);

            for (int i = 0; i < menu.Size(); i++)
                menu.GetItem(i).SetVisible(!drawerOpen);

                return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(global::Android.Views.IMenuItem item)
        {
            if (this._drawerToggle.OnOptionsItemSelected(item))
                return true;

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this._drawerToggle.SyncState();
        }

    }

}