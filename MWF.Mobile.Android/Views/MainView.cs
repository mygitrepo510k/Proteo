using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using Android.Content.PM;
using MWF.Mobile.Android.Helpers;
using MWF.Mobile.Core.ViewModels;
using Android.Support.V4.App;

namespace MWF.Mobile.Android.Views
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainView
        : BaseActivityView, Presenters.IFragmentHost
    {
        #region Private/Protected Fields

        private DrawerLayout _drawer;
        private MvxListView _drawerList;
        private global::Android.Support.V4.App.ActionBarDrawerToggle _drawerToggle;
        protected static IDictionary<Type, Type> _supportedViewModels = new Dictionary<Type, Type>
            {
                { typeof(Core.ViewModels.CameraViewModel),typeof(Fragments.CameraFragment)},
                { typeof(Core.ViewModels.InstructionViewModel), typeof(Fragments.InstructionFragment) },
                { typeof(Core.ViewModels.InstructionCommentViewModel), typeof(Fragments.InstructionCommentFragment)},
                { typeof(Core.ViewModels.InstructionOnSiteViewModel), typeof(Fragments.InstructionOnSiteFragment)},
                { typeof(Core.ViewModels.InstructionSignatureViewModel), typeof(Fragments.InstructionSignatureFragment) },
                { typeof(Core.ViewModels.InstructionTrailerViewModel), typeof(Fragments.InstructionTrailerFragment)},
                { typeof(Core.ViewModels.OrderViewModel), typeof(Fragments.OrderFragment)},
                { typeof(Core.ViewModels.ManifestViewModel), typeof(Fragments.ManifestFragment) },
                { typeof(Core.ViewModels.ReviseQuantityViewModel), typeof(Fragments.ReviseQuantityFragment)}
            };

        #endregion

        #region Construction
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Page_Main);

            this.ActionBar.SetDisplayHomeAsUpEnabled(true);
            this.ActionBar.SetHomeButtonEnabled(true);

            //Navigation Draw
            _drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            _drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

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
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this._drawerToggle.SyncState();
        }

        #endregion

        #region Fragment Host

        public override IDictionary<Type, Type> SupportedFragmentViewModels
        {
            get { return _supportedViewModels; }
        }

        public override int FragmentHostID { get { return Resource.Id.fragment_host_main; } }

        #endregion Fragment host

        #region Options Menu

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
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void InvalidateOptionsMenu()
        {
            _drawer.CloseDrawers();

        }

        #endregion

    }

}