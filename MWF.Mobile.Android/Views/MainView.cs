using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using Android.Content.PM;
using MWF.Mobile.Android.Helpers;

namespace MWF.Mobile.Android.Views
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainView
        : BaseActivityView, Presenters.IFragmentHost
    {

        private DrawerLayout _drawer;
        private MvxListView _drawerList;
        private global::Android.Support.V4.App.ActionBarDrawerToggle _drawerToggle;

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

        protected override Type GetFragmentTypeForViewModel(Type viewModelType)
        {
            return _supportedFragmentViewModels[viewModelType];
        }

        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.main_activity_actions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

		#region Fragment host

        private static IDictionary<Type, Type> _supportedFragmentViewModels = new Dictionary<Type, Type>
        {
            { typeof(Core.ViewModels.ManifestViewModel), typeof(Fragments.ManifestFragment) },
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

        /// <summary>
        /// Given a view model attempts to close the fragment associated with it. If the fragment
        /// associated is currently being displayed in the fragment host then the backstack
        /// is popped.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public bool Close(IMvxViewModel viewModel)
        {
            var fragmentTypeToClose = _supportedFragmentViewModels[viewModel.GetType()];

            if (CurrentFragment != null && CurrentFragment.GetType() == fragmentTypeToClose)
            {
                FragmentManager.PopBackStack();
                return true;
            }
            else return false;
        }

        // Current Fragment in the fragment host. Note although the id being used appears to be that of the 
        // original container, it gets replaced during a show by the new fragment *but* keeps it's old id.
        public MvxFragment CurrentFragment
        {
            get { return FragmentManager.FindFragmentById(Resource.Id.fragment_host) as MvxFragment; }
        }

        public void CloseToInitialView()
        {
            throw new NotImplementedException();
        }

        public void CloseUpToView<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            throw new NotImplementedException();
        }

        #endregion Fragment host
        
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